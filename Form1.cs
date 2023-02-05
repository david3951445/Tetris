using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tetris {
    public partial class Form1 : Form {
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

        public Form1() {
            InitializeComponent();
            init();
            block = generateBlock();
            score = new Score();
            block.print(grids);
            timer1.Start();
    }

        private void init() {
            // gamefield
            AddLabelToTable(grids, tableLayoutPanel2);

            // HOLD and NEXT
            TableLayoutPanel[] table = new TableLayoutPanel[] {
                tableLayoutPanel3,
                tableLayoutPanel5,
                tableLayoutPanel6,
                tableLayoutPanel7,
                tableLayoutPanel8,
                tableLayoutPanel9
            };

            for (int k = 0; k < 6; k++) {
                picGrids[k] = new Label[4, 4];
                AddLabelToTable(picGrids[k], table[k]);
            }

            for (int i = 0; i < 7; i++) {
                blockPics[i] = new Block((BlockTypes)i);
            }

            // local init function
            void AddLabelToTable(Label[,] l, TableLayoutPanel t) {
                for (int i = 0; i < l.GetLength(0); i++) {
                    for (int j = 0; j < l.GetLength(1); j++) {
                        l[i, j] = new Label();
                        l[i, j].BackColor = Color.Black; // background color
                        l[i, j].Dock = DockStyle.Fill;
                        l[i, j].Margin = new Padding(0);

                        t.Controls.Add(l[i, j], j, i);
                    }
                }
            }
        }

        private Block generateBlock() { // Generate blocks with Tetris’s generation rules
            if (num7.Count <= 7) { // Since the rules are 7 different types a cycle, add a cycle if less then 7
                // Generate random no repeat numbers from 0 to 6. Draw Card Method
                List<int> l = new List<int>() {0, 1, 2, 3, 4, 5, 6}; // cards
                for (int i = 7; i > 0; i--) {
                    int rand = r.Next(i); // chose a random card
                    num7.Enqueue(l[rand]); // add it to desired array
                    l.RemoveAt(rand); //draw out
                }
            }

            // show 5 pics on NEXT
            int[] arr = num7.ToArray();
            for (int i = 0; i < 5; i++) {
                blockPics[arr[i]].erase(picGrids[i+1]);
                blockPics[arr[i+1]].print(picGrids[i+1]);
            }
            BlockTypes type = (BlockTypes)num7.Dequeue();
            
            Block b = new Block(type);
            b.pos.set(gameLenX / 2 - 1, 0); // set pos to middle-top of gamefield
            return b;
        }

        private void move(Keys key) {
            block.erase(grids); // erase old block
            switch (key) {
                case Keys.Left: moveLeft(); break;
                case Keys.Right: moveRight(); break;
                case Keys.Down: moveDown(); break;
                case Keys.Up: rotate(); break;
                case Keys.Z: rotateCounter(); break;
                case Keys.C: hold(); break;
                case Keys.Escape: Close(); break;
                case Keys.Space: while (moveDown()); break;
            }
            block.print(grids); // print new block
        }

        public void moveLeft() {
            block.pos.x--;

            if (collision_check() != 0) {
                block.pos.x++; // recovery
            }
        }

        public void moveRight() {
            block.pos.x++;

            if (collision_check() != 0) {
                block.pos.x--; // recovery
            }
        }

        public bool moveDown() {
            block.pos.y++;

            CollisionTypes check = collision_check();
            if (check == CollisionTypes.Bottom || check == CollisionTypes.Gamefield) {
                block.pos.y--; // recovery
                block.print(grids); // recovery

                timer1.Stop();
                timer2.Start();
                lineEliminated_check();
                isHold = false;

                block = generateBlock();
                if (collision_check() != CollisionTypes.Free) {
                    timer1.Stop();
                    MessageBox.Show("GAMEOVER");
                    Close();
                }

                return false;
            }

            return true;
        }

        public void rotate() {
            block.rotate();

            if (collision_check() != CollisionTypes.Free) {
                block.rotateCounter(); // recovery
            }
        }

        public void rotateCounter() {
            block.rotateCounter();

            if (collision_check() != CollisionTypes.Free) {
                block.rotate(); // recovery
            }
        }

        private CollisionTypes collision_check() {
            foreach (coord c in block.shape) {
                int x = block.pos.x + c.x;
                int y = block.pos.y + c.y;

                // boundary
                if (x < 0) return CollisionTypes.Left;
                if (x >= gameLenX) return CollisionTypes.Right;
                if (y < 0) return CollisionTypes.Top;
                if (y >= gameLenY) return CollisionTypes.Bottom;

                // blocks on the field
                if (grids[y, x].BackColor != Color.Black) return CollisionTypes.Gamefield;
            }

            return CollisionTypes.Free; // no collision
        }

        private void hold() {
            if (isHold) {
                return;
            }
            isHold = true;

            BlockTypes old = holdType;
            holdType = block.type; // store type of current block       

            if (old == BlockTypes.NULL) { // there is no hold block (first hold)
                blockPics[(int)holdType].print(picGrids[0]); // print current block on HOLD
                block = generateBlock();
            }
            else {
                blockPics[(int)old].erase(picGrids[0]); // erase old block on HOLD
                blockPics[(int)holdType].print(picGrids[0]); // print current block on HOLD
                block = new Block(old); // put hold block into gamefield
                block.pos.x = gameLenX / 2 - 1;
            }         
        }

        private void lineEliminated_check() {
            // Check if exist line to be eliminated, if not, store it to oldLines[]
            int[] oldLines = new int[gameLenY];
            int k = gameLenY - 1;
            for (int i = gameLenY - 1; i >= 0; i--) {
                int j;
                for (j = 0; j < gameLenX; j++) {
                    if (grids[i, j].BackColor == Color.Black) {
                        break;
                    }
                }

                if (j == gameLenX) { // its a Line Clear

                }
                else { // store lines that do not need to be eliminated
                    oldLines[k--] = i;
                    Console.WriteLine(k);
                }
            }

            if (k > -1) {
                label3.Text = score.Update(block.type, k+1);
            }

            for (int i = gameLenY - 1; i >= 0; i--) {
                for (int j = 0; j < gameLenX; j++) {
                    if (i >= k) { // Still have old lines
                        grids[i, j].BackColor = grids[oldLines[i], j].BackColor;
                    }
                    else {
                        grids[i, j].BackColor = Color.Black; // Pad gamefield's color
                    }
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e) {        
            move(Keys.Down);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e) {
            // temporary method. Let block "stay longer" before block reach end
            if (e.KeyCode == Keys.Down) {
                // Find squares at the bottom of block
                int[] arr = new int[4];
                for (int i = 0; i < 4; i++) {
                    arr[i] = block.shape[i].y;
                }
                Array.Sort(arr);

                // check those squares
                for (int i = 3; i >= 0 && arr[i] == arr[3]; i--) {
                    int x = block.pos.x + block.shape[i].x;
                    int y = block.pos.y + block.shape[i].y + 1;

                    if (y >= gameLenY || grids[y, x].BackColor != Color.Black) {
                        return;
                    }
                }
            }

            move(e.KeyCode);
        }

        private void timer2_Tick(object sender, EventArgs e) {
            timer1.Start();
            timer2.Stop();
        }
    }

    public enum BlockTypes {
        Z, L, O, S, I, J, T, NULL
    }
    public enum CollisionTypes {
        Free, Left, Right, Top, Bottom, BottomMinus, Gamefield
    }

    public class coord {
        public int x, y;
        public coord() {
            x = y = 0;
        }

        public void set(int _x, int _y) {
            x = _x; y = _y;
        }
    };

    public class Block {
        /* Tetris block
            *     All 7 blocks are constructed by 4 "square" but in different shape.
            */

        public Color color = new Color(); // square
        public coord pos = new coord(); // block's "center" position
        public coord[] shape = new coord[4]; // squares' position relative to pos
        public BlockTypes type;

        public Block(BlockTypes _type) {
            switch (_type) {
                case BlockTypes.Z: Block(Color.Red, new int[,] { { 0, 0 }, { 1, 0 }, { 1, 1 }, { 2, 1 } }); break; // Z
                case BlockTypes.L: Block(Color.Orange, new int[,] { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 0, 1 } }); break; // L
                case BlockTypes.O: Block(Color.Yellow, new int[,] { { 0, 0 }, { 1, 0 }, { 0, 1 }, { 1, 1 } }); break; // O
                case BlockTypes.S: Block(Color.Green, new int[,] { { 0, 1 }, { 1, 0 }, { 1, 1 }, { 2, 0 } }); break; // S
                case BlockTypes.I: Block(Color.Blue, new int[,] { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 3, 0 } }); break; // I
                case BlockTypes.J: Block(Color.Indigo, new int[,] { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 2, 1 } }); break; // J
                case BlockTypes.T: Block(Color.Purple, new int[,] { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 1, 1 } }); break; // T
                default: Block(Color.Purple, new int[,] { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 1, 1 } }); break; // T
            }

            type = _type;
            void Block(Color c, int[,] s) { // a local init function
                color = c;
                for (int i = 0; i < 4; i++) {
                    shape[i] = new coord();
                    shape[i].x = s[i, 0];
                    shape[i].y = s[i, 1];
                }
            }
        }

        public void rotate() { // +90 deg
            foreach (coord c in shape) {
                int temp = c.x;
                c.x = -c.y;
                c.y = temp;

                // shift to match rotation rall in Tetris
                switch (type) {
                    case BlockTypes.Z:
                    case BlockTypes.S:
                        c.x += 2;
                        break;
                    case BlockTypes.L:
                    case BlockTypes.O:
                        c.x++;
                        break;
                    case BlockTypes.I:
                    case BlockTypes.J:
                        c.x += 2;
                        c.y--;
                        break;
                    case BlockTypes.T:
                        c.x++;
                        c.y--;
                        break;
                }
            }
        }

        public void rotateCounter() { // -90 deg
            foreach (coord c in shape) {
                int temp = c.x;
                c.x = c.y;
                c.y = -temp;

                // shift to match rotation rall in Tetris
                switch (type) {
                    case BlockTypes.Z:
                    case BlockTypes.S:
                        c.y += 2;
                        break;
                    case BlockTypes.L:
                    case BlockTypes.O:
                        c.y++;
                        break;
                    case BlockTypes.I:
                    case BlockTypes.J:
                        c.x++;
                        c.y += 2;
                        break;
                    case BlockTypes.T:
                        c.x++;
                        c.y++;
                        break;
                }
            }
        }

        public void print(Label[,] table) { // print block on table
            foreach (coord c in shape) {
                int x = pos.x + c.x;
                int y = pos.y + c.y;
                table[y, x].BackColor = color;
            }
        }

        public void erase(Label[,] table) { // erase block on table
            foreach (coord c in shape) {
                int x = pos.x + c.x;
                int y = pos.y + c.y;
                table[y, x].BackColor = Color.Black;
            }
        }
    };

    public class Score {
        public int score = 0; // game score
        public bool B2B = false; // B2B status
        
        public Score() {
        }

        public string Update(BlockTypes type, int line) {
            switch (type) {
                case BlockTypes.T:
                    // temporary method. Need to judge whether is T-spin.
                    score += line * 2; // tetris bonus
                    if (B2B) score++; // B2B bonus
                    B2B = true;
                    break;
                case BlockTypes.I:
                    score += line;
                    if (B2B) score++; // B2B bonus
                    B2B = true;
                    break;
                default:
                    score += line;
                    B2B = false; // B2B is interrupted
                    break;
            }

            return "SCORE : " + score.ToString();
        }
    };
}