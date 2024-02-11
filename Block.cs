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
        Z, L, O, S, I, J, T, NULL
    }

    public class Block
    {
        /* Tetris block
            *     All 7 blocks are constructed by 4 "square" but in different shape.
            */

        public Color color = new Color(); // square
        public Coord pos = new Coord(); // block's "center" position
        public Coord[] shape = new Coord[4]; // squares' position relative to pos
        public BlockTypes type;

        public Block(BlockTypes _type)
        {
            switch (_type)
            {
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
            void Block(Color c, int[,] s)
            { // a local init function
                color = c;
                for (int i = 0; i < 4; i++)
                {
                    shape[i] = new Coord();
                    shape[i].x = s[i, 0];
                    shape[i].y = s[i, 1];
                }
            }
        }

        public void Rotate()
        { // +90 deg
            foreach (Coord c in shape)
            {
                int temp = c.x;
                c.x = -c.y;
                c.y = temp;

                // shift to match rotation rall in Tetris
                switch (type)
                {
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

        public void RotateCounter()
        { // -90 deg
            foreach (Coord c in shape)
            {
                int temp = c.x;
                c.x = c.y;
                c.y = -temp;

                // shift to match rotation rall in Tetris
                switch (type)
                {
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

        public void Print(Label[,] table)
        { // print block on table
            foreach (Coord c in shape)
            {
                int x = pos.x + c.x;
                int y = pos.y + c.y;
                table[y, x].BackColor = color;
            }
        }

        public void Erase(Label[,] table)
        { // erase block on table
            foreach (Coord c in shape)
            {
                int x = pos.x + c.x;
                int y = pos.y + c.y;
                table[y, x].BackColor = Color.Black;
            }
        }
    };

}
