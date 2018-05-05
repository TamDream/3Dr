using System.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Data.SqlClient;
using System.IO;
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;
using HGT;
using GetZernike;
using featurenorm;
using System.Text.RegularExpressions;
using KNNLib;
using System.Timers;
using System.Diagnostics;  
using System.Threading;

namespace _3D物体检索_数据库取值beta
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Thread t1 = new Thread(process1);
            t1.Start();
        }

        public void process1()
        {
            string str2 = Environment.CurrentDirectory;
            str2 = str2 + "\\test.jpg";
            zernikex(str2);
            hogx(str2);
            featurex(str2);
            Console.WriteLine("over");
        }
        //数据处理集的arraylist
        ArrayList V = new ArrayList();
        ArrayList Z = new ArrayList();
        ArrayList C = new ArrayList();
        ArrayList H = new ArrayList();
        //featurefusion
        ArrayList F = new ArrayList();
        DataTable sqldt = new DataTable();

        //page fuction
        int pagenum = 1;
        int[] sortedobj = new int[1000];


        int sqltype = 0;
        int k = 0;
        //int matchfitnum = 16; //匹配数字标记1
        int matchnum = 18;//匹配数字标记2
        int matchtype = 0;
        string featuretype;


        OpenFileDialog ofd1 = new OpenFileDialog();//打开图片
        OpenFileDialog ofd2 = new OpenFileDialog();
        OpenFileDialog ofd3 = new OpenFileDialog();
        OpenFileDialog ofd4 = new OpenFileDialog();
        //删除或添加图片位置
        //int[] weizhi = new int[14] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        int[] yweizhi = new int[14];
      
        int[] tianjia = new int[14];

        //翻页所需要的全局变量
        int[] tupianshu = new int[2];/////////////////数组
      
        public string[] path;//定义检索路径
        public string[] pathbackup1;
        public string[] pathbackup2;
        public string jiaohuan;

        Stopwatch sw = new Stopwatch();

        double[] xiangsidu = new double[1000];
        double[] xiangsidu1 = new double[1000];
        double[] xiangsidu2 = new double[1000];
        double[] xiangsidu3 = new double[1000];

        //////////////////////////////////////////新增全局变量/////////////////////////////////////////////////////////
        public string[] shanchupath;//检索结果删除路径后的图片排序
        int[] sametu = new int[10] { -1, -1, -1, -1, -1, -1, -1, 1, -1, -1 };//检索结果删除图片要用到
      
        string[] tx = new string[4] { "null", "null", "null", "null" };
        
        //////////////////////////////////////////////////////////////////////////////
        ///图像像素数////
        int pixelnumber = 25;
        //用于判断所选算法
      

        ///////////////////////直方图提取所用变量/////////////////////////////
        string str;/////临时存储颜色名称  即color p1.name 十六进制数
      
        int intervallength = Convert.ToInt32(Math.Pow(256, 3) / 2000);////////直方图每个区间长度
        

        int intervalname;////////////////////区间名称临时变量
      
        int n1;
        ///////////////////////余弦距离所用变量///////////////////
        
        //////////名字提取///////////////
        string[,] classname = new string[10000, 10];////////类名数组/////////////
        string[,] objectname = new string[10000, 10];  ///////物体名数组
        string[,] dealpicname = new string[10000, 10];  ///////图像名数组  第二维记录是类型库1还是待检测2
        //////////////////////////////////颜色直方图相关图像缩小函数//////////////////

        int [] resultnum=new int [1000];

        int retrievalsignal = 0;

        //重检索部分变量
        int[] selectednum = new int[3];
        int cnumber = 0;
        string[] selectedpath = new string[3];
        string resultstr = null;
        string[] datasortstr = new string[3];
        int[] sortedobj2 = new int[1000];
        int reretrievalsignal = 0;

        public double[] simprocess(double[] sim, int matchnum)
        {
            string []resultx = new string[8];
            int StartIndex = 0;
            int EndIndex = 9;
            double[] classsim = new double[8];
            for (int i = 0; i < classsim.Length; i++)
            {
                double[] sim2 = SplitArray(sim, StartIndex, EndIndex);
                sim2 = sim2.OrderBy(x => x).ToArray();
                classsim[i] = sim2[matchnum];//////豪斯多夫改为sim2[9],最近邻改为sim2[0]
                StartIndex += 10;
                EndIndex += 10;
            }
          
            return classsim;
        }

        public double[] SplitArray(double[] Source, int StartIndex, int EndIndex)
        {
            try
            {
                double[] result = new double[EndIndex - StartIndex + 1];
                for (int i = 0; i < EndIndex - StartIndex + 1; i++) result[i] = Source[i + StartIndex];
                return result;
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #region combinelist()
        public ArrayList combinelist(int combinenum, string type)
        {
            double[] temporary = new double[1000];
            int objnum = combinenum % 10;
            int classnum = combinenum / 10;
            System.Console.WriteLine(classnum + "x" + objnum);
            int picnum = 0;
            ArrayList M = new ArrayList();
            while (picnum < 41)
            {
                temporary = Sqlsource(type, picnum, objnum, classnum);
                String cl = "" + (objnum+(classnum*10));
                M.Add(new Obj { ID = "0", Class_label = cl, Attributes = new Attr_Arr(temporary) });
                picnum++;
            }
            //System.Console.WriteLine( "done!" );
            return M;
        }
        #endregion


        #region Sqlsource (string type, int picnum,int objnum,string numtype)
        public double[] Sqlsource(string type, int picnum, int objnum, int classnum)
        {
            String Textfea = "";          
                if (type == "zernike") //0是类号 1是物体号 2是图片号 以上是int 
                //3是color 4是hog 5是zernike 7是path 以上是string
                { 
                    Textfea = (string)sqldt.Rows[(41 * objnum) + (410 * classnum) + picnum][5];
                }
               
                if(type == "hog")
                { 
                    Textfea = (string)sqldt.Rows[(41 * objnum) + (410 * classnum) + picnum][4];
                }
                
                    if (type == "zerhog")
                    {
                        Textfea = (string)sqldt.Rows[(41 * objnum) + (410 * classnum) + picnum][3];
                    }
                string[] T = Textfea.Split(new char[] { 'x', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                List<string> listx = new List<string>();
                foreach (string ax in T)
                {
                    if (!string.IsNullOrEmpty(ax))
                    {
                        listx.Add(ax);
                    }
                }
                string[] sf = listx.ToArray();
                List<double> douhog = sf.ToList<string>().Select(n => Convert.ToDouble(n)).ToList<double>();
                double[] eigenvalue = douhog.ToArray();
                return eigenvalue;
            //}
        }
        #endregion

        #region  string Sqlpath(int num)
        public string Sqlpath(int num)
        {           
            Random ran = new Random();
            int picnum = ran.Next(0, 40);
            int objnum = num % 10;
            int classnum = num / 10;
            String Textfea = "";
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = "Data Source=.;Initial Catalog=Pic;Integrated Security=True";
                conn.Open();
                using (SqlCommand command = conn.CreateCommand())
                {
                    //if (sqltype == 2)
                    //{
                    //    command.CommandText = "select *  from PSB where classnum = " + classnum + "and objectnum =" + objnum + "and picnum =" + picnum;
                    //}
                    //else
                    {
                        command.CommandText = "select *  from imagex where classnum = " + classnum + "and objectnum =" + objnum + "and picnum =" + picnum;
                    }
                
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string data = reader.GetString(reader.GetOrdinal("path"));

                        Textfea = data;

                    }
                }


                return Textfea;
            }
        }

        #endregion

        #region  string Sqltablex(string sqlname,string tablename)
        public string sqltablex(string sqlname, string tablename)
        {
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = "Data Source=.;Initial Catalog="+sqlname+";Integrated Security=True";
                conn.Open();
                using (SqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = "select * from "+tablename;
                    SqlDataAdapter da = new SqlDataAdapter(command);

                    try
                    {
                        da.Fill(ds);
                    }
                    catch
                    { }
                }
            }

            // this.dataGridView1.DataSource = sqldt;  //绑定到datagridview中显示
            sqldt = ds.Tables[0].Copy();
            return null;
        }
         #endregion

        #region 算法
        ////////////////////////////////////////////////////////////算法//////////////////////////////////////////////////////////////////////////



        #region  GetNewImage(String s, int newWidth, int newHeight)
        public static Image GetNewImage(String s, int newWidth, int newHeight)
        {
            Image oldImg = Image.FromFile(s);//加载原图像
            Image newImg = oldImg.GetThumbnailImage(newWidth, newHeight, new Image.GetThumbnailImageAbort(IsTrue), IntPtr.Zero);
            //对原图像进行缩放
            return newImg;
        }
        #endregion


        #region IsTrue()
        private static bool IsTrue()//在Image类别对图片进行缩放时，需要一个bool类别的委托
        {
            return true;
        }
        #endregion


      


    


        #region hogx(String strHeadImagePath)
        double[] hogx(String strHeadImagePath)
        {
            double[] hogx = new double[128];
            HGT.hclass th = new HGT.hclass();

            MWArray[] argsout = new MWArray[1];
            MWArray[] argsin = new MWArray[] { strHeadImagePath }; //这里strHeadImagePath是文件路径  数据来源！！！！！
            th.HOG(1, ref argsout, argsin);         //matlab算法  黑箱子算法
            MWNumericArray x1 = argsout[0] as MWNumericArray;
            String s_x1 = x1.ToString();
            string[] sf;
            string[] s = s_x1.Split(new char[] { ' ', ' ' });

            //System.Console.WriteLine("  hog: " + s_x1);


            List<string> list = new List<string>();
            foreach (string ax in s)
            {
                if (!string.IsNullOrEmpty(ax))
                {
                    list.Add(ax);
                }
            }
            sf = list.ToArray();
            List<double> douhog = sf.ToList<string>().Select(n => Convert.ToDouble(n)).ToList<double>();
            //double[] abc = new double[1000];
            //douhog.CopyTo(abc, 0);

            int m = 0;

            while (m < douhog.Count)//
            {

                hogx[m] = douhog[m];

                m++;
            }

            return hogx;
        }
        #endregion


        #region zernikex(String ImagePath)
        double[] zernikex(String ImagePath)
        {
            GetZernike.zclass tz = new GetZernike.zclass();
            MWArray[] argsout = new MWArray[1];
            MWArray[] argsin = new MWArray[] { ImagePath }; //这里strHeadImagePath是文件路径  数据来源！！！！！

            tz.DB_GetZernike(1, ref argsout, argsin);

            //tz.zernike7(1, ref argsout, argsin);

            MWNumericArray x1 = argsout[0] as MWNumericArray;

            String s_x1 = x1.ToString();

            string[] s = Regex.Split(s_x1, "  ", RegexOptions.IgnoreCase);



            //this.textBox1.Text += "特征为(x1) " + s[0] + " y1 ) " + s[1] + " x2  ) " + s[2] + " y2 ) " + s[3];
            //List<double> dous = s.ToList<string>().Select(n => Convert.ToDouble(n)).ToList<double>();
            //System.Console.WriteLine("  d1:  " + dous[0] + "  d2:  " + dous[1] + " d1+d2 " + (dous[0] + dous[1]));
            string[] sf;
            string[] sa = s_x1.Split(new char[] { ' ', ' ' });

            //System.Console.WriteLine("  d1:" + s_x1);


            List<string> list = new List<string>();
            foreach (string ax in sa)
            {
                if (!string.IsNullOrEmpty(ax))
                {
                    list.Add(ax);
                }
            }
            sf = list.ToArray();
            //this.textBox1.Text += "特征为(1) " + sf[0] + " 2 ) " + sf[1] + " 3  ) " + sf[2] + " 4 ) " + sf[3];
            List<double> douz = sf.ToList<string>().Select(n => Convert.ToDouble(n)).ToList<double>();
            // System.Console.WriteLine("  d1:  " + douz[0] + "  d2:  " + douz[1] + " (d1+d2) " + (douz[0] + douz[1]));


            int m = 0;
            double[] zernikearray = new double[49];
            while (m < douz.Count)//
            {
                //Console.WriteLine(douzerk[m] + "!");
                zernikearray[m] = douz[m];
                m++;
            }

            return zernikearray;

        }
        #endregion//

    

        #endregion
        double[] featurex(String ImagePath)
        {

            featurenorm.Class1 ff = new featurenorm.Class1();
            MWArray[] argsout = new MWArray[1];
            MWArray[] argsin = new MWArray[] { ImagePath }; //这里strHeadImagePath是文件路径  数据来源！！！！！

            ff.featurenorm(1, ref argsout, argsin);

            //tz.zernike7(1, ref argsout, argsin);

            MWNumericArray x1 = argsout[0] as MWNumericArray;

            String s_x1 = x1.ToString();

            string[] s = Regex.Split(s_x1, "  ", RegexOptions.IgnoreCase);



            //this.textBox1.Text += "特征为(x1) " + s[0] + " y1 ) " + s[1] + " x2  ) " + s[2] + " y2 ) " + s[3];
            //List<double> dous = s.ToList<string>().Select(n => Convert.ToDouble(n)).ToList<double>();
            //System.Console.WriteLine("  d1:  " + dous[0] + "  d2:  " + dous[1] + " d1+d2 " + (dous[0] + dous[1]));
            string[] sf;
            string[] sa = s_x1.Split(new char[] { ' ', ' ' });

            //System.Console.WriteLine("  d1:" + s_x1);


            List<string> list = new List<string>();
            foreach (string ax in sa)
            {
                if (!string.IsNullOrEmpty(ax))
                {
                    list.Add(ax);
                }
            }
            sf = list.ToArray();
            //this.textBox1.Text += "特征为(1) " + sf[0] + " 2 ) " + sf[1] + " 3  ) " + sf[2] + " 4 ) " + sf[3];
            List<double> douz = sf.ToList<string>().Select(n => Convert.ToDouble(n)).ToList<double>();
            // System.Console.WriteLine("  d1:  " + douz[0] + "  d2:  " + douz[1] + " (d1+d2) " + (douz[0] + douz[1]));


            int m = 0;
            double[] fusionarray = new double[177];
            while (m < douz.Count)//
            {
                //Console.WriteLine(douzerk[m] + "!");
                fusionarray[m] = douz[m];
                m++;
            }

            return fusionarray;
        }

        //private void hausdorff1_CheckedChanged(object sender, EventArgs e)
        //{
           
        //}

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        #region pagerefresh(int page)
        //页面刷新@@
        public string pagerefresh(int page)
        {
            if (reretrievalsignal == 1) 
            { }
            else 
            {
                //1
                if (selectednum.Contains(0 + (8 * (pagenum))))
                {
                    label1.Visible = true;

                }
                else
                {
                    label1.Visible = false;
                }
                //label1.Refresh();

                //2
                if (selectednum.Contains(1 + (8 * (pagenum))))
                {
                    label4.Visible = true;
                }
                else
                {
                    label4.Visible = false;
                }
                //label4.Refresh();

                //3
                if (selectednum.Contains(2 + (8 * (pagenum))))
                {
                    label7.Visible = true;
                }
                else
                {
                    label7.Visible = false;
                }
                //label7.Refresh();

                //4
                if (selectednum.Contains(3 + (8 * (pagenum))))
                {
                    label8.Visible = true;
                }
                else
                {
                    label8.Visible = false;
                }
                //label8.Refresh();

                //5
                if (selectednum.Contains(4 + (8 * (pagenum))))
                {
                    label9.Visible = true;
                }
                else
                {
                    label9.Visible = false;
                }
                //label9.Refresh();

                //6
                if (selectednum.Contains(5 + (8 * (pagenum))))
                {
                    label10.Visible = true;
                }
                else
                {
                    label10.Visible = false;
                }
                //label10.Refresh();

                //7
                if (selectednum.Contains(6 + (8 * (pagenum))))
                {
                    label11.Visible = true;
                }
                else
                {
                    label11.Visible = false;
                }
                //label11.Refresh();

                //8
                if (selectednum.Contains(7 + (8 * (pagenum))))
                {
                    label12.Visible = true;
                }
                else
                {
                    label12.Visible = false;
                }
                //label12.Refresh();
            }
            

            return null;
        }
        #endregion


        public void re_select(string foldPath,int countfoldnum)
        {
            string axa = Path.GetDirectoryName(foldPath);
            String[] files = Directory.GetFiles(axa, "*.png", SearchOption.TopDirectoryOnly);//*.jpg
            if (files.Length == 0)
            {
                files = Directory.GetFiles(axa, "*.jpg", SearchOption.TopDirectoryOnly);
            }
            Z.Clear();
            H.Clear();
            int m = 0;
            while (m < files.Length)
            {
                sw.Start();
                Console.WriteLine(files[m] + " ! ");
                Z.Add(new Obj { ID = "0", Class_label = "blank", Attributes = new Attr_Arr(zernikex(files[m])) });
                H.Add(new Obj { ID = "0", Class_label = "blank", Attributes = new Attr_Arr(hogx(files[m])) });//featurex(String ImagePath)
                F.Add(new Obj { ID = "0", Class_label = "blank", Attributes = new Attr_Arr(featurex(files[m])) });
                m++;
                sw.Stop();
                Console.WriteLine("打开物体 " + sw.Elapsed);
                //Application.DoEvents();
                this.progressBar1.Value = ((m * 100) / (files.Length));
                progressBar1.Refresh();
                this.label3.Text = "选择的第" + countfoldnum + "个物体重检索进度： " + ((m * 100) / (files.Length)) + " %";
                label3.Refresh();
            }
            this.label3.Text = "选择的第" + countfoldnum + "个物体重检索进度： " + "已完成";
            label3.Refresh();
            //if ((sqltype == 1) || (sqltype == 2))
            //{
            if ((matchtype == 3) || (matchtype == 4) || (matchtype == 5))
            {

                double[] classsim = new double[10];
                Stopwatch sww = new Stopwatch();

                SortedList<double, int> sortedList = new SortedList<double, int>();


                double[] SIM = new double[1800];

                sw.Reset();
                sw.Start();

                ////豪斯多夫改为0，最近邻改为100
                int trainingobj = 0;
                ArrayList Q = new ArrayList();
                if (matchtype == 3) { Q = H; }

                if (matchtype == 4) { Q = Z; }

                if (matchtype == 5) { Q = F; }
                Obj[] test_obj = (Obj[])Q.ToArray(typeof(Obj));

                sw.Stop();
                Console.WriteLine("arraylist的复制 " + sw.Elapsed);
                //if ((matchnum == 0) || (matchnum == 9))
                //{
                sww.Start();
                int[] x = new int[80];
                while (trainingobj < 80)
                {
                    sw.Reset();
                    sw.Start();
                    ArrayList P = combinelist(trainingobj, featuretype);
                    Obj[] training_set = (Obj[])P.ToArray(typeof(Obj));

                    if ((matchnum == 9) || (matchnum == 0))
                    {

                        SIM[trainingobj] = new KNNLib.KNN(ref training_set, ref test_obj, k).KNNCluster();////k=1最近邻       ///k=2hausdorff
                        // SIMbackup[trainingobj] = SIM[trainingobj];
                    }
                    else if ((matchnum == 11))
                    {

                        SIM[trainingobj] = new KNNLib.KNN(ref training_set, ref test_obj, k).HausdroffCluster();

                    }
                    sw.Stop();
                    Console.WriteLine("匹配算法第" + trainingobj + "次的时间为 " + sw.Elapsed);

                    sw.Reset();
                    sw.Start();

                    sortedList.Add(SIM[trainingobj], trainingobj);

                    sw.Stop();
                    Console.WriteLine("SortedList第" + trainingobj + "次的时间为 " + sw.Elapsed);

                    System.Console.WriteLine("No." + trainingobj + " sim " + SIM[trainingobj]);


                    trainingobj++;
                }


                sww.Stop();
                sortedobj2 = sortedList.Values.ToArray();



                /*
                for (int le = 0; le < 80; le++)
                {
                    Console.WriteLine(sortedobj2[le]);
                }*/
                int a = 0, b = 0;
                for (; b < 8; b++)
                {
                    while (a < 80)
                    {
                        if (sortedobj2[b] == sortedobj[a])
                        {
                            for (int c = a; c < 79; c++)
                            {
                                sortedobj[c] = sortedobj[c + 1];
                            }
                            sortedobj[79] = sortedobj2[b];
                            a = 80;
                        }
                        a++;
                    }
                    a = 0;
                }


                pictureBox5.Image = Image.FromFile(Sqlpath(sortedobj[0])); tupianshu[0] = 1;
                pictureBox5.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                pictureBox6.Image = Image.FromFile(Sqlpath(sortedobj[1]));
                pictureBox6.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                pictureBox7.Image = Image.FromFile(Sqlpath(sortedobj[2]));
                pictureBox7.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                pictureBox8.Image = Image.FromFile(Sqlpath(sortedobj[3]));
                pictureBox8.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                pictureBox9.Image = Image.FromFile(Sqlpath(sortedobj[4]));
                pictureBox9.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                pictureBox10.Image = Image.FromFile(Sqlpath(sortedobj[5]));
                pictureBox10.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                pictureBox11.Image = Image.FromFile(Sqlpath(sortedobj[6]));
                pictureBox11.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                pictureBox12.Image = Image.FromFile(Sqlpath(sortedobj[7]));
                pictureBox12.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间    


                //}
            }
        }
        


        private void button1_Click_1(object sender, EventArgs e)//打开待检索图像
        {

            Random ran = new Random();
            Z.Clear();
            H.Clear();
            F.Clear();
            FolderBrowserDialog dialog = new FolderBrowserDialog();//设置检索路径
            dialog.Description = "请选择文件路径";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string foldPath = dialog.SelectedPath;
               // Console.WriteLine(" \n " + foldPath + " \n ");
                String[] files = Directory.GetFiles(foldPath, "*.png", SearchOption.TopDirectoryOnly);//*.jpg
                if (files.Length==0)
                {
                   files = Directory.GetFiles(foldPath, "*.jpg", SearchOption.TopDirectoryOnly);
                }

               // sw.Start();

                int m = 0;
                while (m < files.Length)
                {
                    sw.Start();
                    Console.WriteLine(files[m] + " ! ");
                    Z.Add(new Obj { ID = "0", Class_label = "blank", Attributes = new Attr_Arr(zernikex(files[m])) });
                    H.Add(new Obj { ID = "0", Class_label = "blank", Attributes = new Attr_Arr(hogx(files[m])) });
                    F.Add(new Obj { ID = "0", Class_label = "blank", Attributes = new Attr_Arr(featurex(files[m])) });
                    m++;
                    sw.Stop();
                    Console.WriteLine("打开物体 " + sw.Elapsed);
                    this.progressBar1.Value = ((m * 100) / (files.Length));
                    progressBar1.Refresh();
                    this.label3.Text = "正在提取物体特征，进度：" + ((m * 100) / (files.Length)) + "%";
                    label3.Refresh();
                }
                this.label3.Text = "状态：已载入物体";
                label3.Refresh();
                //sw.Stop();
                sqltablex("Pic", "imagex");
                //Console.WriteLine("打开物体 " + sw.Elapsed);

                sw.Reset();
                sw.Start();

                int picn = ran.Next(0, files.Length);
                pictureBox1.Image = Image.FromFile(files[picn]);//装图片
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;//
                picn = ran.Next(0, files.Length);
                pictureBox2.Image = Image.FromFile(files[picn]);//装图片
                pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;//
                picn = ran.Next(0, files.Length);
                pictureBox3.Image = Image.FromFile(files[picn]);//装图片
                pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;//
                picn = ran.Next(0, files.Length);
                pictureBox4.Image = Image.FromFile(files[picn]);//装图片
                pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;//

                sw.Stop();
                Console.WriteLine("填图片 "+sw.Elapsed);
            }

            pictureBox5.Image = null;
            pictureBox6.Image = null;
            pictureBox7.Image = null;
            pictureBox8.Image = null;
            pictureBox9.Image = null;
            pictureBox10.Image = null;
            pictureBox11.Image = null;
            pictureBox12.Image = null;
            knn1.Checked = false;
            zernikeradioButton.Checked = false;
            HOGradioButton.Checked = false;
            hausdorff1.Checked = false;
            retrievalsignal = 0;
            button3.Visible = true;
            groupBox1.Visible = true;
            groupBox2.Visible = true;
            groupBox5.Visible = false;
            groupBox6.Visible = false;
            groupBox7.Visible = false;
        }

        private void button3_Click_1(object sender, EventArgs e)//数据库提取
        {
           
            if ((matchtype == 3) || (matchtype == 4) || (matchtype == 5))
                {
                   
                      double[] classsim = new double[10];
                      Stopwatch sww = new Stopwatch();
                     
                      SortedList<double, int> sortedList = new SortedList<double, int>();
             
            
                      double[] SIM = new double[1800];

                      sw.Reset();
                      sw.Start();

                      int trainingobj = 0;
                      ArrayList Q = new ArrayList();
                      if (matchtype == 3) { Q = H; }

                      if (matchtype == 4) { Q = Z; }
                      if (matchtype == 5) { Q = F; }
                      Obj[] test_obj = (Obj[])Q.ToArray(typeof(Obj));

                      sw.Stop();
                      Console.WriteLine("arraylist的复制 " + sw.Elapsed);
                     
                          sww.Start();
                          int[] x = new int[80];
                          while (trainingobj < 80)
                          {
                              sw.Reset();
                              sw.Start();
                              ArrayList P = combinelist(trainingobj, featuretype);
                              Obj[] training_set = (Obj[])P.ToArray(typeof(Obj));

                              if ((matchnum == 0))
                              {

                                  SIM[trainingobj] = new KNNLib.KNN(ref training_set, ref test_obj, k).KNNCluster();
                                  
                              }
                              else if ((matchnum == 9))
                              {
                                 
                                  SIM[trainingobj] = new KNNLib.KNN(ref training_set, ref test_obj, k).HausdroffCluster();
                                
                              }
                              sw.Stop();
                              Console.WriteLine("匹配算法第" + trainingobj + "次的时间为 " + sw.Elapsed);

                              sw.Reset();
                              sw.Start();

                              sortedList.Add(SIM[trainingobj], trainingobj);

                              sw.Stop();
                              Console.WriteLine("SortedList第" + trainingobj + "次的时间为 " + sw.Elapsed);

                              System.Console.WriteLine("No." + trainingobj + " sim " + SIM[trainingobj]);


                              trainingobj++;
                          }


                          sww.Stop();
                          Console.WriteLine("总 " + sww.Elapsed);
                          this.label3.Text = "状态：已读取";


                          sortedobj = sortedList.Values.ToArray();
                          ///////////////将相似度前14的物体投影至窗口//////////////////////
                          pictureBox5.Image = Image.FromFile(Sqlpath(sortedobj[0])); tupianshu[0] = 1; yweizhi[0] = 1;
                          pictureBox5.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                          pictureBox6.Image = Image.FromFile(Sqlpath(sortedobj[1])); yweizhi[1] = 2;
                          pictureBox6.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                          pictureBox7.Image = Image.FromFile(Sqlpath(sortedobj[2])); yweizhi[2] = 3;
                          pictureBox7.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                          pictureBox8.Image = Image.FromFile(Sqlpath(sortedobj[3])); yweizhi[3] = 4;
                          pictureBox8.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                          pictureBox9.Image = Image.FromFile(Sqlpath(sortedobj[4])); yweizhi[4] = 5;
                          pictureBox9.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                          pictureBox10.Image = Image.FromFile(Sqlpath(sortedobj[5])); yweizhi[5] = 6;
                          pictureBox10.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                          pictureBox11.Image = Image.FromFile(Sqlpath(sortedobj[6])); yweizhi[6] = 7;
                          pictureBox11.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                          pictureBox12.Image = Image.FromFile(Sqlpath(sortedobj[7])); yweizhi[7] = 8;
                          pictureBox12.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                          retrievalsignal = 1;
                        

                      //}
            }
                groupBox5.Visible = false;
                groupBox6.Visible = false;
                groupBox7.Visible = false;
           //}
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ETH_CheckedChanged(object sender, EventArgs e)
        {
            sqltype = 1;
        }

        private void PSB_CheckedChanged(object sender, EventArgs e)
        {
            sqltype = 2;
        }

        

        private void hausdorff1_CheckedChanged_1(object sender, EventArgs e)
        {
            k = 9;
            matchnum = 9;
        }
        private void knn1_CheckedChanged(object sender, EventArgs e)
        {
            k = 9;
            matchnum = 0;
        }
       

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void HOGradioButton_CheckedChanged(object sender, EventArgs e)
        {
            matchtype = 3;
            featuretype = "hog";
        }

        private void zernikeradioButton_CheckedChanged(object sender, EventArgs e)
        {
            matchtype = 4;
            featuretype = "zernike";
        }

        private void ZerhogradioButton_CheckedChanged(object sender, EventArgs e)
        {
            matchtype = 5;
            featuretype = "zerhog";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (retrievalsignal == 1)
            {
              if (pagenum == 1) { MessageBox.Show("目前已到了第一页！"); }
              else
              {
                pagenum--;
                pagerefresh(pagenum);
                pictureBox5.Image = Image.FromFile(Sqlpath(sortedobj[0 + (8 * (pagenum - 1))]));
                pictureBox5.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                pictureBox6.Image = Image.FromFile(Sqlpath(sortedobj[1 + (8 * (pagenum - 1))]));
                pictureBox6.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                pictureBox7.Image = Image.FromFile(Sqlpath(sortedobj[2 + (8 * (pagenum - 1))]));
                pictureBox7.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                pictureBox8.Image = Image.FromFile(Sqlpath(sortedobj[3 + (8 * (pagenum - 1))]));
                pictureBox8.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                pictureBox9.Image = Image.FromFile(Sqlpath(sortedobj[4 + (8 * (pagenum - 1))]));
                pictureBox9.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                pictureBox10.Image = Image.FromFile(Sqlpath(sortedobj[5 + (8 * (pagenum - 1))]));
                pictureBox10.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                pictureBox11.Image = Image.FromFile(Sqlpath(sortedobj[6 + (8 * (pagenum - 1))]));
                pictureBox11.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                pictureBox12.Image = Image.FromFile(Sqlpath(sortedobj[7 + (8 * (pagenum - 1))]));
                pictureBox12.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                label13.Text = "第" + pagenum + "页";
              }
            }
            else
            {
                MessageBox.Show("你还没有进行过一次3D检索，请先对选择物体进行检索。");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (retrievalsignal == 1)
            {
                if (pagenum == 10) { MessageBox.Show("目前已到了最后一页！"); }
                else
                {
                    pagenum++;
                    pagerefresh(pagenum);
                    pictureBox5.Image = Image.FromFile(Sqlpath(sortedobj[0 + (8 * (pagenum - 1))]));
                    pictureBox5.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                    pictureBox6.Image = Image.FromFile(Sqlpath(sortedobj[1 + (8 * (pagenum - 1))]));
                    pictureBox6.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                    pictureBox7.Image = Image.FromFile(Sqlpath(sortedobj[2 + (8 * (pagenum - 1))]));
                    pictureBox7.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                    pictureBox8.Image = Image.FromFile(Sqlpath(sortedobj[3 + (8 * (pagenum - 1))]));
                    pictureBox8.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                    pictureBox9.Image = Image.FromFile(Sqlpath(sortedobj[4 + (8 * (pagenum - 1))]));
                    pictureBox9.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                    pictureBox10.Image = Image.FromFile(Sqlpath(sortedobj[5 + (8 * (pagenum - 1))]));
                    pictureBox10.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                    pictureBox11.Image = Image.FromFile(Sqlpath(sortedobj[6 + (8 * (pagenum - 1))]));
                    pictureBox11.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                    pictureBox12.Image = Image.FromFile(Sqlpath(sortedobj[7 + (8 * (pagenum - 1))]));
                    pictureBox12.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                    label13.Text = "第" + pagenum + "页";
                }
            }
            else
            {
                MessageBox.Show("你还没有进行过一次3D检索，请先对选择物体进行检索。");
            }
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            if (cnumber < 3)
            {
                label1.Visible = true;
                selectedpath[cnumber] = Sqlpath(sortedobj[0 + (8 * (pagenum - 1))]);
                selectednum[cnumber] = 0 + (8 * (pagenum));
                cnumber++;
            }
            else 
            {
                MessageBox.Show("为了保证运行时间，每次物体重检索不能选择三个以上的物体，谢谢合作。");
            }
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            if (cnumber < 3)
            {
                label4.Visible = true;
                selectedpath[cnumber] = Sqlpath(sortedobj[1 + (8 * (pagenum - 1))]);
                selectednum[cnumber] = 1 + (8 * (pagenum ));
                cnumber++;
            }
            else
            {
                MessageBox.Show("为了保证运行时间，每次物体重检索不能选择三个以上的物体，谢谢合作。");
            }
           
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            if (cnumber < 3)
            {
                label7.Visible = true;
                selectedpath[cnumber] = Sqlpath(sortedobj[2 + (8 * (pagenum - 1))]);
                selectednum[cnumber] = 2 + (8 * (pagenum));
                cnumber++;
            }
            else
            {
                MessageBox.Show("为了保证运行时间，每次物体重检索不能选择三个以上的物体，谢谢合作。");
            }
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            if (cnumber < 3)
            {
                label8.Visible = true;
                selectedpath[cnumber] = Sqlpath(sortedobj[3 + (8 * (pagenum - 1))]);
                selectednum[cnumber] =3 + (8 * (pagenum ));
                cnumber++;
            }
            else
            {
                MessageBox.Show("为了保证运行时间，每次物体重检索不能选择三个以上的物体，谢谢合作。");
            }
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            if (cnumber < 3)
            {
                label9.Visible = true;
                selectedpath[cnumber] = Sqlpath(sortedobj[4 + (8 * (pagenum - 1))]);
                selectednum[cnumber] = 4 + (8 * (pagenum ));
                cnumber++;
            }
            else
            {
                MessageBox.Show("为了保证运行时间，每次物体重检索不能选择三个以上的物体，谢谢合作。");
            }
        }

        private void pictureBox10_Click(object sender, EventArgs e)
        {
            if (cnumber < 3)
            {
                label10.Visible = true;
                selectedpath[cnumber] = Sqlpath(sortedobj[5 + (8 * (pagenum - 1))]);
                selectednum[cnumber] = 5 + (8 * (pagenum ));
                cnumber++;
            }
            else
            {
                MessageBox.Show("为了保证运行时间，每次物体重检索不能选择三个以上的物体，谢谢合作。");
            }
        }

        private void pictureBox11_Click(object sender, EventArgs e)
        {
            if (cnumber < 3)
            {
                label11.Visible = true;
                selectedpath[cnumber] = Sqlpath(sortedobj[6 + (8 * (pagenum - 1))]);
                selectednum[cnumber] = 6 + (8 * (pagenum));
                cnumber++;
            }
            else
            {
                MessageBox.Show("为了保证运行时间，每次物体重检索不能选择三个以上的物体，谢谢合作。");
            }
        }

        private void pictureBox12_Click(object sender, EventArgs e)
        {
            if (cnumber < 3)
            {
                label12.Visible = true;
                selectedpath[cnumber] = Sqlpath(sortedobj[7 + (8 * (pagenum - 1))]);
                selectednum[cnumber] = 7 + (8 * (pagenum));
                cnumber++;
            }
            else
            {
                MessageBox.Show("为了保证运行时间，每次物体重检索不能选择三个以上的物体，谢谢合作。");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if ((retrievalsignal != 1))
            {
                MessageBox.Show("你还没有进行过一次3D检索！");
                knn1.Checked = false;
                HOGradioButton.Checked = false;
                hausdorff1.Checked = false;
                zernikeradioButton.Checked = false;
                
            }
            else
            {
                int m = 0,countm=0;
                while (m < selectedpath.Length)
                {
                    if (selectedpath[m] != null)
                    {
                        re_select(selectedpath[m], m + 1);
                        countm++;
                    }
                    m++;
                }
                if (countm == 1)
                {
                    groupBox5.Visible = true;
                    pictureBox13.Visible = true;
                    pictureBox13.Image = Image.FromFile(selectedpath[0]);
                    pictureBox13.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                }

                if (countm == 2)
                    {
                        groupBox5.Visible = true;
                        pictureBox13.Visible = true;
                        pictureBox13.Image = Image.FromFile(selectedpath[0]);
                        pictureBox13.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                        groupBox6.Visible = true;
                        pictureBox14.Visible = true;
                        pictureBox14.Image = Image.FromFile(selectedpath[1]);
                        pictureBox14.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                    }

                if (countm == 3)
                    {
                        groupBox5.Visible = true;
                        pictureBox13.Visible = true;
                        pictureBox13.Image = Image.FromFile(selectedpath[0]);
                        pictureBox13.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                        groupBox6.Visible = true;
                        pictureBox14.Visible = true;
                        pictureBox14.Image = Image.FromFile(selectedpath[1]);
                        pictureBox14.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                        groupBox7.Visible = true;
                        pictureBox15.Visible = true;
                        pictureBox15.Image = Image.FromFile(selectedpath[2]);
                        pictureBox15.SizeMode = PictureBoxSizeMode.StretchImage;//占满整个空间
                    }
                //selectednum.Initialize();
                pagenum = 1;
                label13.Text = "第1页";
                label1.Visible = false;
                label4.Visible = false;
                label7.Visible = false;
                label8.Visible = false;
                label9.Visible = false;
                label10.Visible = false;
                label11.Visible = false;
                label12.Visible = false;
                reretrievalsignal = 1;
                button3.Visible = false;
                groupBox1.Visible = false;
                groupBox2.Visible = false;
            }
            
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        
    }
}


