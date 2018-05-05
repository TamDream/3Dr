using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KNNLib
{
    public class Obj
    {
        /// <summary>
        /// 样本ID
        /// </summary>
        private string id;
        public string ID
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }
        /// <summary>
        /// 样本类标签
        /// </summary>
        private string class_label;
        public string Class_label
        {
            get
            {
                return this.class_label;
            }
            set
            {
                this.class_label = value;
            }
        }
        /// <summary>
        /// 类号码标签
        /// </summary>
        private int classnum;
        public int CLASSNUM
        {
            get
            {
                return this.classnum;
            }
            set
            {
                this.classnum = value;
            }
        }


        /// <summary>
        /// 物体号码标签
        /// </summary>
        private int objectnum;
        public int OBJECTNUM
        {
            get
            {
                return this.objectnum;
            }
            set
            {
                this.objectnum = value;
            }
        }


        /// <summary>
        /// 图片号码标签
        /// </summary>
        private int picnum;
        public int PICNUM
        {
            get
            {
                return this.picnum;
            }
            set
            {
                this.picnum = value;
            }
        }

        /// <summary>
        /// 样本属性
        /// </summary>
        private IAttribute attributes;
        public IAttribute Attributes
        {
            get
            {
                return this.attributes;
            }
            set
            {
                this.attributes = value;
            }
        }

        /// <summary>
        /// 记录与其他样本间的距离
        /// </summary>
        private double sim;
        public double Sim
        {
            get
            {
                return this.sim;
            }
            set
            {
                this.sim = value;
            }
        }


    }
}
