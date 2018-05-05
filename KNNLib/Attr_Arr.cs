using System;

namespace KNNLib
{
    public class Attr_Arr : IAttribute
    {
        /// <summary>
        /// n维属性值
        /// </summary>
        private double[] values;
        public double[] Values
        {
            get
            {
                return this.values;
            }
        }

        /// <summary>
        /// 直接用参数构建
        /// </summary>
        /// <param name="values"></param>
        public Attr_Arr(params double[] values)
        {
            this.values = values;
        }

        /// <summary>
        /// 计算与另一个二维属性的相似度，可以通过重写应用于多维数据
        /// 或者通过重写更换相似度度量公式
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        public virtual double Sim(IAttribute attr)
        {
            Attr_Arr attr_arr = (Attr_Arr)attr;
            int dim = attr_arr.Values.Length;
            double sum = 0;
           // Console.WriteLine(this.values.Length + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" + attr_arr.Values.Length);
            for (int i = 0; i < dim; i++)
            {
                
                sum += Math.Pow(Convert.ToDouble(this.values[i]) -Convert.ToDouble(attr_arr.Values[i]), 2.0);
            }
            return Math.Sqrt(sum);
        }
    }
}
