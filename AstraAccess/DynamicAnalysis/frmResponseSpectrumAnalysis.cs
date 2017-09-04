using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using AstraInterface.Interface;
using AstraInterface.DataStructure;

namespace AstraAccess.DynamicAnalysis
{
    public partial class frmResponseSpectrumAnalysis : Form
    {
        IApplication iApp;

        public string user_path { get; set; }
        string Input_File { get; set; }

        public eASTRADesignType ProjectType { get; set; }
        public string Title
        {
            get
            {
                if (ProjectType == eASTRADesignType.Response_Spectrum_Analysis) return "RESPONSE SPECTRUM ANALYSIS";
                if (ProjectType == eASTRADesignType.Time_History_Analysis) return "TIME HISTORY ANALYSIS";
                return "EIGEN VALUE ANALYSIS";
            }
        }

        public frmResponseSpectrumAnalysis(IApplication app, eASTRADesignType projType)
        {
            InitializeComponent();
            iApp = app;
            ProjectType = projType;

            txt_project_name.Text = iApp.Set_Project_Name(Title);

            Text = Title + " [" + MyList.Get_Modified_Path(iApp.LastDesignWorkingFolder) + "]";

        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == btn_new_design)
            {
                New_Design();
            }
            else if (btn == btn_open_design)
            {
                Open_Design();
            }
            else if (btn == btn_input_browse)
            {
                Input_File_Browse();
            }
            else  if (btn == btn_create_input_data)
            {
                Create_Input_Data();

            }
            else if (btn == btn_process_analysis)
            {
                Process_Analysis();

            }
            else if (btn == btn_open_analysis_report)
            {
                Open_Analysis_Report();
            }
            Save_Data();
        }


        private void Process_Analysis()
        {
           if( iApp.RunAnalysis(Input_File))
           {

               string rep = MyList.Get_Analysis_Report_File(Input_File);
               if (File.Exists(rep)) rtb_analysis_results.Lines = File.ReadAllLines(rep);
           }
        }

        public void Save_Data()
        {
            iApp.Save_Form_Record(this, user_path);
        }
        private void New_Design()
        {
            string ss = iApp.Create_Project(Title, txt_project_name.Text, eASTRADesignType.Response_Spectrum_Analysis);
            if (ss != "") user_path = ss;

            iApp.Save_Form_Record(this, user_path);
        }

        private void Open_Design()
        {
            string ss = iApp.Open_Project_Dialog(this.Name, Path.Combine(iApp.LastDesignWorkingFolder, Title));
            if (ss != "")
            {
                user_path = ss;
                iApp.Read_Form_Record(this, user_path);

            }
        }
        private void Input_File_Browse()
        {
            string ext = "";
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "ASTRA Project Files (*.apr)|*.apr|Input Text File (*.txt)|*.txt";
                if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
                {
                    ext = Path.GetExtension(ofd.FileName).ToLower();
                    if (ext == ".txt")
                    {
                        rtb_input_data.Lines = File.ReadAllLines(ofd.FileName);
                        txt_input_data.Text = Path.GetFileName(ofd.FileName);
                        string ll_txt = Path.Combine(Path.GetDirectoryName(ofd.FileName), "LL.TXT");
                        if(File.Exists(ll_txt))
                        {
                            File.Copy(ll_txt, Path.Combine(user_path, "LL.TXT"), true);
                        }

                    }
                    else if( ext == ".apr")
                    {
                        Select_From_APR_File(ofd.FileName);
                    }
                }
            }
            Create_Input_Data();
        }
        public void Select_From_APR_File(string apr_file)
        {
            string dir = Path.GetDirectoryName(apr_file);

            string flName = Path.GetFileNameWithoutExtension(apr_file);


            string pth = Path.Combine(dir, flName);

            List<string> allFiles = new List<string>();

            if(Directory.Exists(pth))
            {
                foreach (var item in Directory.GetFiles(pth))
                {

                    if (Path.GetExtension(item).ToLower() == ".txt")
                    {
                        allFiles.Add(item);
                    }
                }
            }
            else
            {
                foreach (var item in Directory.GetFiles(dir))
                {
                    if (Path.GetExtension(item).ToLower() == ".txt")
                    {
                        allFiles.Add(item);
                    }
                }
            }

            pth = Path.Combine(dir, flName);

            if(allFiles.Count > 0)
            {
                foreach (var item in allFiles)
                {
                    pth = Path.GetFileName(item).ToUpper();
                    if(pth.StartsWith("INPUT"))
                    {
                        txt_input_data.Text = pth;
                        rtb_input_data.Lines = File.ReadAllLines(pth);
                        string ll_txt = Path.Combine(Path.GetDirectoryName(pth), "LL.TXT");
                        if (File.Exists(ll_txt))
                        {
                            File.Copy(ll_txt, Path.Combine(user_path, "LL.TXT"), true);
                        }
                        break;
                    }

                }
            }

        }

        private void Create_Input_Data()
        {

            Input_File = Path.Combine(user_path, txt_input_data.Text);

            File.WriteAllLines(Input_File, rtb_input_data.Lines);

            MessageBox.Show("Analysis Input Data File created as " + Input_File, "ASTRA", MessageBoxButtons.OK);
        }

        private void Open_Analysis_Report()
        {
            string rep = MyList.Get_Analysis_Report_File(Input_File);


            if (File.Exists(rep)) System.Diagnostics.Process.Start(rep);
        }

        private void frmResponseSpectrumAnalysis_Load(object sender, EventArgs e)
        {
            splitContainer1.SplitterDistance = 450;
        }


    }
}
