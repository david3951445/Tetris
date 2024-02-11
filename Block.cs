using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tetris
{
    public enum BlockTypes
    {
        Z,
        L,
        O,
        S,
        I,
        J,
        T,
        NULL
    }

    public class Block
    {
        /* Tetris block
            *     All 7 blocks are constructed by 4 "square" but in different shape.
            */

        private Color _color = new Color(); // square
        public Coord Pos = new Coord(); // block's "center" position
        public Coord[] Shape = new Coord[4]; // squares' position relative to pos
        public BlockTypes Type;

        public Block(BlockTypes type)
        {
            switch (type)
            {
                case BlockTypes.Z: Init(Color.Red, new int[,] { { 0, 0 }, { 1, 0 }, { 1, 1 }, { 2, 1 } }); break; // Z
                case BlockTypes.L: Init(Color.Orange, new int[,] { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 0, 1 } }); break; // L
                case BlockTypes.O: Init(Color.Yellow, new int[,] { { 0, 0 }, { 1, 0 }, { 0, 1 }, { 1, 1 } }); break; // O
                case BlockTypes.S: Init(Color.Green, new int[,] { { 0, 1 }, { 1, 0 }, { 1, 1 }, { 2, 0 } }); break; // S
                case BlockTypes.I: Init(Color.Blue, new int[,] { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 3, 0 } }); break; // I
                case BlockTypes.J: Init(Color.Indigo, new int[,] { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 2, 1 } }); break; // J
                case BlockTypes.T: Init(Color.Purple, new int[,] { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 1, 1 } }); break; // T
                default: Init(Color.Purple, new int[,] { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 1, 1 } }); break; // T
            }

            Type = type;
        }

        private void Init(Color c, int[,] s)
        {
            _color = c;
            for (int i = 0; i < 4; i++)
                Shape[i] = new Coord(s[i, 0], s[i, 1]);
        }

        public void Rotate()
        { // +90 deg
            foreach (Coord c in Shape)
            {
                int temp = c.X;
                c.X = -c.Y;
                c.Y = temp;

                // shift to match rotation rall in Tetris
                switch (Type)
                {
                    case BlockTypes.Z:
                    case BlockTypes.S:
                        c.X += 2;
                        break;
                    case BlockTypes.L:
                    case BlockTypes.O:
                        c.X++;
                        break;
                    case BlockTypes.I:
                    case BlockTypes.J:
                        c.X += 2;
                        c.Y--;
                        break;
                    case BlockTypes.T:
                        c.X++;
                        c.Y--;
                        break;
                }
            }
        }

        public void RotateCounter()
        { // -90 deg
            foreach (Coord c in Shape)
            {
                int temp = c.X;
                c.X = c.Y;
                c.Y = -temp;

                // shift to match rotation rall in Tetris
                switch (Type)
                {
                    case BlockTypes.Z:
                    case BlockTypes.S:
                        c.Y += 2;
                        break;
                    case BlockTypes.L:
                    case BlockTypes.O:
                        c.Y++;
                        break;
                    case BlockTypes.I:
                    case BlockTypes.J:
                        c.X++;
                        c.Y += 2;
                        break;
                    case BlockTypes.T:
                        c.X++;
                        c.Y++;
                        break;
                }
            }
        }

        public void Print(Label[,] table)
        { // print block on table
            foreach (Coord c in Shape)
            {
                int x = Pos.X + c.X;
                int y = Pos.Y + c.Y;
                table[y, x].BackColor = _color;
            }
        }

        public void Erase(Label[,] table)
        { // erase block on table
            foreach (Coord c in Shape)
            {
                int x = Pos.X + c.X;
                int y = Pos.Y + c.Y;
                table[y, x].BackColor = Color.Black;
            }
        }
    };

}
