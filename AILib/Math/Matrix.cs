﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AILib.Math
{
    [Serializable]
    public class Matrix
    {
        private float[][] data;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Matrix(int w, int h)
        {
            data = new float[w][];
            for (int i = 0; i < w; i++) data[i] = new float[h];
            Width = w;
            Height = h;
        }

        public static void MSub(Matrix matrix1, float rate, ref Matrix matrix2)
        {
            matrix2 -= matrix1 * rate;
        }

        private Matrix()
        {

        }

        public float this[int x, int y]
        {
            get
            {
                return data[x][y];
            }
            set
            {
                data[x][y] = value;
            }
        }

        public static Matrix operator *(Matrix a, Matrix b)
        {
            Matrix c = new Matrix(b.Width, a.Height);

            if (a.Width != b.Height)
                throw new ArgumentException();

            //dot(Row(a), Column(b))
            for (int j = 0; j < c.Width; j++)
                for (int k = 0; k < a.Width; k++)
                {
                    float b_d = b.data[j][k];

                    float[] cj = c.data[j];
                    float[] ak = a.data[k];


                    /*for(int i = 0; i < c.Height; i++)
                    {
                        cj[i] += ak[i] * b_d;
                    }*/


                    int i = 0;
                    int simdLength = Vector<float>.Count;
                    for (i = 0; i <= c.Height - simdLength; i += simdLength)
                    {
                        var res = new Vector<float>(ak, i) * b_d + new Vector<float>(cj, i);
                        res.CopyTo(cj, i);
                    }
                    for (; i < c.Height; ++i)
                    {
                        cj[i] += ak[i] * b_d;
                    }
                }

            return c;
        }

        public static void Multiply(Matrix a, Matrix b, ref Matrix c)
        {
            c = a * b;
        }

        public static Matrix operator -(Matrix a, Matrix b)
        {
            Matrix c = new Matrix(a.Width, a.Height);

            if (a.Height != b.Height)
                throw new ArgumentException();

            if (a.Width != b.Width)
                throw new ArgumentException();

            for (int j = 0; j < c.Width; j++)
            {
                /*
                for (int j = 0; j < c.Height; j++)
                    c.data[i][j] = a.data[i][j] - b.data[i][j];
                    */


                int i = 0;
                int simdLength = Vector<float>.Count;
                for (i = 0; i <= c.Height - simdLength; i += simdLength)
                {
                    var res = new Vector<float>(a.data[j], i) - new Vector<float>(b.data[j], i);
                    res.CopyTo(c.data[j], i);
                }
                for (; i < c.Height; ++i)
                {
                    c.data[j][i] = a.data[j][i] - b.data[j][i];
                }
            }

            return c;
        }

        public static Vector operator *(Matrix a, Vector b)
        {
            float[] tmp = new float[a.Height];

            for (int j = 0; j < a.Width; j++)
            {
                float b_v = b[j];

                int i = 0;
                int simdLength = Vector<float>.Count;
                for (i = 0; i <= tmp.Length - simdLength; i += simdLength)
                {
                    var res = new Vector<float>(a.data[j], i) * b_v;
                    res.CopyTo(tmp, i);
                }
                for (; i < tmp.Length; ++i)
                {
                    tmp[i] += a.data[j][i] * b_v;
                }
            }

            return new Vector(tmp);
        }

        public static void Madd(Matrix matrix, Vector res1, Vector vector, ref Vector res2)
        {
            res2 = (matrix * res1) + vector;
        }

        public static Matrix operator *(Matrix a, float b)
        {
            Matrix c = new Matrix(a.Width, a.Height);

            for (int j = 0; j < c.Width; j++)
            {
                /*for (int j = 0; j < c.Height; j++)
                    c.data[i][j] = a.data[i][j] * b;*/


                int i = 0;
                int simdLength = Vector<float>.Count;
                for (i = 0; i <= c.Height - simdLength; i += simdLength)
                {
                    var res = new Vector<float>(a.data[j], i) * b;
                    res.CopyTo(c.data[j], i);
                }
                for (; i < c.Height; ++i)
                {
                    c.data[j][i] = a.data[j][i] * b;
                }
            }

            return c;
        }

        public static Matrix Hadamard(Matrix a, Matrix b)
        {
            if (a.Width != 1 | b.Width != 1)
                throw new ArgumentException();

            if (a.Height != b.Height)
                throw new ArgumentException();

            Matrix res = new Matrix(1, a.Height);

            /*for(int i = 0; i < a.Height; i++)
            {
                res.data[0][i] = a.data[0][i] * b.data[0][i];
            }*/


            int i = 0;
            int simdLength = Vector<float>.Count;
            for (i = 0; i <= res.Height - simdLength; i += simdLength)
            {
                var res_v = new Vector<float>(a.data[0], i) * new Vector<float>(b.data[0], i);
                res_v.CopyTo(res.data[0], i);
            }
            for (; i < res.Height; ++i)
            {
                res.data[0][i] = a.data[0][i] * b.data[0][i];
            }

            return res;
        }

        public static void TransposedMultiply(Matrix a, Vector b, Vector c, ref Vector res)
        {
            if (a.Height != b.Length)
                throw new ArgumentException();

            //Vector res = new Vector(a.Width);

            for (int i = 0; i < res.Length; i++)
            {
                res[i] = Vector.Dot(new Vector(a.data[i]), Vector.Hadamard(b, c));
            }

            //return res;
        }
        
        public static void MultiplyToMatrix(Vector a, Vector b, ref Matrix r)
        {
            //Matrix r = new Matrix(b.Length, a.Length);
            for (int i = 0; i < r.Width; i++)
            {
                r.data[i] = a * b[i];
            }
            //return r;
        }

        public static Matrix Transpose(Matrix a)
        {
            Matrix m = new Matrix(a.Height, a.Width);

            for (int i = 0; i < a.Width; i++)
                for (int j = 0; j < a.Height; j++)
                {
                    m.data[j][i] = a.data[i][j];
                }
            return m;
        }

        public static explicit operator Matrix(Vector a)
        {
            Matrix m = new Matrix(1, a.Length);
            for (int i = 0; i < a.Length; i++)
                m.data[0][i] = a[i];
            return m;
        }
    }
}
