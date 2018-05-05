using System.Collections.Generic;
using System.Linq;
using System;

namespace KNNLib
{
    public class KNN
    {
        /// <summary>
        /// KNN参数k
        /// </summary>
        private int k;
        public int K
        {
            set
            {
                this.k = value;
            }
        }
        /// <summary>
        /// 训练集
        /// </summary>
        private Obj[] training_set;
        public Obj[] Training_set
        {
            set
            {
                this.training_set = value;
            }
        }

        /// <summary>
        /// 测试样本，可不赋值
        /// 若不赋值，务必使用有参数的主函数
        /// </summary>
        private Obj[] test_obj;
        public Obj[] Test_obj
        {
            set
            {
                this.test_obj = value;
            }
        }

        private Obj[] test_pic;
        public Obj[] Test_pic
        {
            set
            {
                this.test_pic = value;
            }
        }

        /// <summary>
        /// 构建函数
        /// </summary>
        /// <param name="training_set"></param>
        /// <param name="test_obj"></param>
        /// <param name="k"></param>
        public KNN(ref Obj[] training_set, ref Obj[] test_obj, int k)
        {
            this.training_set = training_set;
             this.test_pic = test_obj; 
             this.test_obj = test_obj;
            this.k = k;
        }

        /// <summary>
        /// 执行KNN算法，得到测试样本的类别标签
        /// </summary>
        /// <returns></returns>
        public double KNNCluster()
        {
            double [ ]n =new double [training_set.Length];
            int i = 0;
            for (int j = 0; j < test_obj.Length; j++)
            {
                this.SimCalc(test_obj[j]);
                Obj[] query = this.training_set.OrderBy(obj => obj.Sim).ToArray();
                n[i] = query[0].Sim;
               
               // Console.WriteLine(n[i] + "xxxxxxxxxxxxxxxx"); 
                i++;
            }   
            n = n.OrderBy(x => x).ToArray();
            if (this.k == 1) return n[0];////最近邻      
            else  return n[n.Length - 1];////huasdorff
        }
  
       

        /// <summary>
        /// 令测试样本与所有训练集计算距离
        /// 这里把距离计算延迟给了属性值对象
        /// </summary>
        private void SimCalc(Obj obj)
        {
            for (int i = 0; i < training_set.Length; i++)
            {
                this.training_set[i].Sim = this.training_set[i].Attributes.Sim(obj.Attributes);
            }
        }

        /// <summary>
        /// 执行Hausdroff算法，得到测试样本的类别标签
        /// </summary>
        /// <returns></returns>
        public double HausdroffCluster()
        {
           
            for (int j = 0; j < test_pic.Length; j++)
            {
                test_pic[j].Sim = this.testSimcalc(test_pic[j]);
            }
            Obj[] querytest = this.test_pic.OrderBy(obj => obj.Sim).ToArray();
            double n = querytest[test_pic.Length - 1].Sim;
            //Console.WriteLine(n);

            for (int l = 0; l < training_set.Length; l++)
            {
                training_set[l].Sim = this.trainSimcalc(training_set[l]);

            }
            Obj[] querytrain = this.training_set.OrderBy(obj => obj.Sim).ToArray();
            double m = querytrain[querytrain.Length - 1].Sim;
            //Console.WriteLine("m"+m+"n"+n);
            if (m > n)
                return m;
            else
                return n;

        }



        /// <summary>
        /// 令测试样本与所有训练集计算距离
        /// 这里把距离计算延迟给了属性值对象
        /// </summary>
        private double testSimcalc(Obj obj)
        {
            double[] n = new double[training_set.Length];
            for (int i = 0; i < training_set.Length; i++)
            {
             
                n[i] = this.training_set[i].Attributes.Sim(obj.Attributes);

            }
            n = n.OrderBy(x => x).ToArray();
            return n[0];

        }
        private double trainSimcalc(Obj obj)
        {
            double[] n = new double[test_pic.Length];
            for (int i = 0; i < test_pic.Length; i++)
            {
                n[i] = this.test_pic[i].Attributes.Sim(obj.Attributes);
            }
            n = n.OrderBy(x => x).ToArray();
            return n[0];
        }
    }
}
