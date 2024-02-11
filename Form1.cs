using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tetris
{
    public partial class Form1 : Form
    {
        Random r = new Random();
        Queue<int> num7 = new Queue<int>();
        // gamefield
        const int gameLenX = 10;
        const int gameLenY = 20;
        int gameScore = 0;
        Label[,] grids = new Label[gameLenY, gameLenX]; // background gamefield
        Block block = null; // block on gamefield

        // HOLD and NEXT
        Label[][,] picGrids = new Label[6][,]; // background of HOLD + NEXT*5
        BlockTypes holdType = BlockTypes.NULL; // block on HOLD
        BlockTypes[] nextType = new BlockTypes[5]; // block on NEXT
        Block[] blockPics = new Block[7]; // 7 types of block, constant (treat like pictures)
        bool isHold = false;

        Score score = null;

        public Form1()
        {
            InitializeComponent();
            Init();
            block = GenerateBlock();
            score = new Score();
            block.Print(grids);
            timer1.Start();
        }

        private void Init()
        {
            // gamefield
            AddLabelToTable(grids, tableLayoutPanel2);

            // HOLD and NEXT
            TableLayoutPanel[] table = new TableLayoutPanel[]
            {
                tableLayoutPanel3,
                tableLayoutPanel5,
                tableLayoutPanel6,
                tableLayoutPanel7,
                tableLayoutPanel8,
                tableLayoutPanel9
            };

            for (int k = 0; k < 6; k++)
            {
                picGrids[k] = new Label[4, 4];
                AddLabelToTable(picGrids[k], table[k]);
            }

            for (int i = 0; i < 7; i++)
                blockPics[i] = new Block((BlockTypes)i);

            // local init function
            void AddLabelToTable(Label[,] l, TableLayoutPanel t)
            {
                for (int i = 0; i < l.GetLength(0); i++)
                    for (int j = 0; j < l.GetLength(1); j++)
                    {
                        l[i, j] = new Label();
                        l[i, j].BackColor = Color.Black; // background color
                        l[i, j].Dock = DockStyle.Fill;
                        l[i, j].Margin = new Padding(0);

                        t.Controls.Add(l[i, j], j, i);
                    }
            }
        }

        private Block GenerateBlock() // Generate blocks with Tetris’s generation rules
        {
            if (num7.Count <= 7) // Since the rules are 7 different types a cycle, add a cycle if less then 7
            {
                // Generate random no repeat numbers from 0 to 6. Draw Card Method
                List<int> l = new List<int>() { 0, 1, 2, 3, 4, 5, 6 }; // cards
                for (int i = 7; i > 0; i--)
                {
                    int rand = r.Next(i); // chose a random card
                    num7.Enqueue(l[rand]); // add it to desired array
                    l.RemoveAt(rand); //draw out
                }
            }

            // show 5 pics on NEXT
            int[] arr = num7.ToArray();
            for (int i = 0; i < 5; i++)
            {
                blockPics[arr[i]].Erase(picGrids[i + 1]);
                blockPics[arr[i + 1]].Print(picGrids[i + 1]);
            }
            BlockTypes type = (BlockTypes)num7.Dequeue();

            Block b = new Block(type);
            b.Pos.Set(gameLenX / 2 - 1, 0); // set pos to middle-top of gamefield
            return b;
        }

        private void Move(Keys key)
        {
            block.Erase(grids); // erase old block
            switch (key)
            {
                case Keys.Left: MoveLeft(); break;
                case Keys.Right: MoveRight(); break;
                case Keys.Down: MoveDown(); break;
                case Keys.Up: Rotate(); break;
                case Keys.Z: RotateCounter(); break;
                case Keys.C: Hold(); break;
                case Keys.Escape: Close(); break;
                case Keys.Space: while (MoveDown()) ; break;
            }
            block.Print(grids); // print new block
        }

        public void MoveLeft()
        {
            block.Pos.X--;
            if (CheckCollision() != 0)
                block.Pos.X++;
        }

        public void MoveRight()
        {
            block.Pos.X++;
            if (CheckCollision() != 0)
                block.Pos.X--;
        }

        public bool MoveDown()
        {
            block.Pos.Y++;

            CollisionTypes check = CheckCollision();
            if (check == CollisionTypes.Bottom || check == CollisionTypes.Gamefield)
            {
                block.Pos.Y--; // recovery
                block.Print(grids); // recovery

                timer1.Stop();
                timer2.Start();
                EliminateLine();
                isHold = false;

                block = GenerateBlock();
                if (CheckCollision() != CollisionTypes.Free)
                {
                    timer1.Stop();
                    MessageBox.Show("GAMEOVER");
                    Close();
                }

                return false;
            }

            return true;
        }

        public void Rotate()
        {
            block.Rotate();

            if (CheckCollision() != CollisionTypes.Free)
                block.RotateCounter(); // recovery
        }

        public void RotateCounter()
        {
            block.RotateCounter();

            if (CheckCollision() != CollisionTypes.Free)
                block.Rotate(); // recovery
        }

        private CollisionTypes CheckCollision()
        {
            foreach (Coord c in block.Shape)
            {
                int x = block.Pos.X + c.X;
                int y = block.Pos.Y + c.Y;

                // boundary
                if (x < 0)
                    return CollisionTypes.Left;
                if (x >= gameLenX)
                    return CollisionTypes.Right;
                if (y < 0)
                    return CollisionTypes.Top;
                if (y >= gameLenY)
                    return CollisionTypes.Bottom;

                // blocks on the field
                if (grids[y, x].BackColor != Color.Black)
                    return CollisionTypes.Gamefield;
            }

            return CollisionTypes.Free; // no collision
        }

        private void Hold()
        {
            if (isHold)
                return;
            isHold = true;

            BlockTypes old = holdType;
            holdType = block.Type; // store type of current block       

            if (old == BlockTypes.NULL) // there is no hold block (first hold)
            {
                blockPics[(int)holdType].Print(picGrids[0]); // print current block on HOLD
                block = GenerateBlock();
            }
            else
            {
                blockPics[(int)old].Erase(picGrids[0]); // erase old block on HOLD
                blockPics[(int)holdType].Print(picGrids[0]); // print current block on HOLD
                block = new Block(old); // put hold block into gamefield
                block.Pos.X = gameLenX / 2 - 1;
            }
        }

        private void EliminateLine()
        {
            // Check if exist line to be eliminated, if not, store it to oldLines[]
            var remainedLines = new List<int>();
            for (int indexOfY = gameLenY - 1; indexOfY >= 0; indexOfY--)
            {
                int indexOfX;
                for (indexOfX = 0; indexOfX < gameLenX; indexOfX++)
                    if (grids[indexOfY, indexOfX].BackColor == Color.Black)
                        break;

                if (indexOfX == gameLenX) // its a Line Clear
                {

                }
                else // store lines that do not need to be eliminated
                    remainedLines.Add(indexOfY);
            }

            if (remainedLines.Any())
                label3.Text = score.Update(block.Type, gameLenY - remainedLines.Count);

            for (int i = 0; i < gameLenY; i++)
                for (int j = 0; j < gameLenX; j++)
                {
                    var indexFromBottom = gameLenY - 1 - i;
                    if (i < remainedLines.Count) // Still have old lines
                        grids[indexFromBottom, j].BackColor = grids[remainedLines[i], j].BackColor;
                    else
                        grids[indexFromBottom, j].BackColor = Color.Black; // Pad gamefield's color
                }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            Move(Keys.Down);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // temporary method. Let block "stay longer" before block reach end
            if (e.KeyCode == Keys.Down)
            {
                // Find squares at the bottom of block
                int[] arr = new int[4];
                for (int i = 0; i < 4; i++)
                    arr[i] = block.Shape[i].Y;
                Array.Sort(arr);

                // check those squares
                for (int i = 3; i >= 0 && arr[i] == arr[3]; i--)
                {
                    int x = block.Pos.X + block.Shape[i].X;
                    int y = block.Pos.Y + block.Shape[i].Y + 1;

                    if (y >= gameLenY || grids[y, x].BackColor != Color.Black)
                        return;
                }
            }

            Move(e.KeyCode);
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            timer1.Start();
            timer2.Stop();
        }
    }

    public enum CollisionTypes
    {
        Free,
        Left,
        Right,
        Top,
        Bottom,
        BottomMinus,
        Gamefield
    }

    public class Coord
    {
        public int X;
        public int Y;

        public Coord() { }

        public Coord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void Set(int _x, int _y)
        {
            X = _x; Y = _y;
        }
    };


    public class Score
    {
        private int _score = 0; // game score
        private bool B2B = false; // B2B status

        public string Update(BlockTypes type, int line)
        {
            switch (type)
            {
                case BlockTypes.T:
                    // temporary method. Need to judge whether is T-spin.
                    _score += line * 2; // tetris bonus
                    if (B2B)
                        _score++; // B2B bonus
                    B2B = true;
                    break;
                case BlockTypes.I:
                    _score += line;
                    if (B2B)
                        _score++; // B2B bonus
                    B2B = true;
                    break;
                default:
                    _score += line;
                    B2B = false; // B2B is interrupted
                    break;
            }

            return "SCORE : " + _score.ToString();
        }
    };
}