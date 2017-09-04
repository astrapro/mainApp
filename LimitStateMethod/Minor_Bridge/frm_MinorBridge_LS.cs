﻿using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

using AstraFunctionOne.BridgeDesign.SteelTruss;
using AstraFunctionOne.BridgeDesign;
using AstraInterface.DataStructure;
using AstraInterface.Interface;
using AstraInterface.TrussBridge;

using BridgeAnalysisDesign.Abutment;
using BridgeAnalysisDesign.Pier;
using BridgeAnalysisDesign.RCC_T_Girder;

using BridgeAnalysisDesign;
using LimitStateMethod.LS_Progress;
using LimitStateMethod.Bearing;
using LimitStateMethod.RCC_T_Girder;


namespace LimitStateMethod.Minor_Bridge
{
    public partial class frm_MinorBridge_LS : Form
    {
        //const string Title = "ANALYSIS OF RCC T-GIRDER BRIDGE (LIMIT STATE METHOD)";


        public string Title
        {
            get
            {
                if(iApp.DesignStandard == eDesignStandard.BritishStandard)
                    return "RCC MINOR BRIDGE LIMIT STATE [BS]";
                return "RCC MINOR BRIDGE LIMIT STATE [IRC]";
            }
        }




        IApplication iApp;
        RCC_Minor_LS_Analysis Minor_Analysis = null;
        LS_DeckSlab_Analysis Deck_Analysis = null;

        TLong_SectionProperties long_inner_sec;
        TLong_SectionProperties long_out_sec;
        TCross_SectionProperties cross_sec;

        TGirder_Section_Properties LG_INNER_MID;
        TGirder_Section_Properties LG_OUTER_MID;
        TGirder_Section_Properties LG_INNER_SUP;
        TGirder_Section_Properties LG_OUTER_SUP;
        TGirder_Section_Properties CG_INTER;
        TGirder_Section_Properties CG_END;

        //Chiranjit [2012 06 08]
        RccPier rcc_pier = null;

        //Chiranjit [2012 05 27]
        RCC_AbutmentWall Abut = null;

        bool IsCreateData = true;

        #region Chiranjit [2014 03 12] Support Input
        public string Start_Support_Text
        {
            get
            {
                string kStr = "PINNED";
                if (rbtn_ssprt_pinned.Checked)
                    kStr = "PINNED";
                else if (rbtn_ssprt_fixed.Checked)
                {
                    kStr = "FIXED";


                    if (chk_ssprt_fixed_FX.Checked
                        || chk_ssprt_fixed_FY.Checked
                        || chk_ssprt_fixed_FZ.Checked
                        || chk_ssprt_fixed_MX.Checked
                        || chk_ssprt_fixed_MY.Checked
                        || chk_ssprt_fixed_MZ.Checked)
                        kStr += " BUT";

                    if (chk_ssprt_fixed_FX.Checked) kStr += " FX";
                    if (chk_ssprt_fixed_FY.Checked) kStr += " FY";
                    if (chk_ssprt_fixed_FZ.Checked) kStr += " FZ";
                    if (chk_ssprt_fixed_MX.Checked) kStr += " MX";
                    if (chk_ssprt_fixed_MY.Checked) kStr += " MY";
                    if (chk_ssprt_fixed_MZ.Checked) kStr += " MZ";
                }
                return kStr;
            }
        }
        public string END_Support_Text
        {
            get
            {
                string kStr = "PINNED";
                if (rbtn_esprt_pinned.Checked)
                    kStr = "PINNED";
                else if (rbtn_esprt_fixed.Checked)
                {
                    kStr = "FIXED";
                    if (chk_esprt_fixed_FX.Checked
                        || chk_esprt_fixed_FY.Checked
                        || chk_esprt_fixed_FZ.Checked
                        || chk_esprt_fixed_MX.Checked
                        || chk_esprt_fixed_MY.Checked
                        || chk_esprt_fixed_MZ.Checked)
                        kStr += " BUT";
                    if (chk_esprt_fixed_FX.Checked) kStr += " FX";
                    if (chk_esprt_fixed_FY.Checked) kStr += " FY";
                    if (chk_esprt_fixed_FZ.Checked) kStr += " FZ";
                    if (chk_esprt_fixed_MX.Checked) kStr += " MX";
                    if (chk_esprt_fixed_MY.Checked) kStr += " MY";
                    if (chk_esprt_fixed_MZ.Checked) kStr += " MZ";
                }
                return kStr;
            }
        }
        #endregion Chiranjit [2014 03 12] Support Input


        public int DL_LL_Comb_Load_No
        {
            get
            {
                return MyList.StringToInt(txt_dl_ll_comb.Text, 1);
            }
        }
        //bool IsInnerGirder
        public frm_MinorBridge_LS(IApplication thisApp)
        {
            InitializeComponent();
            iApp = thisApp;
            user_path = iApp.LastDesignWorkingFolder;
            this.Text = Title + " : " + MyList.Get_Modified_Path(user_path);
            Result = new List<string>();

            //Chiranjit [2016 08 09] Add Project Name
            Project_Name = "";



            LG_INNER_MID = new TGirder_Section_Properties();
            LG_OUTER_MID = new TGirder_Section_Properties();
            LG_INNER_SUP = new TGirder_Section_Properties();
            LG_OUTER_SUP = new TGirder_Section_Properties();
            CG_INTER = new TGirder_Section_Properties();
            CG_END = new TGirder_Section_Properties();
        }

        #region Define Properties Chiranjit [2013 09 23]
        //Define Properties
        public double L { get { return MyList.StringToDouble(txt_Ana_L.Text, 26.0); } set { txt_Ana_L.Text = value.ToString("f3"); } }
        public double B { get { return MyList.StringToDouble(txt_Ana_B.Text, 0.0); } set { txt_Ana_B.Text = value.ToString("f3"); } }
        public double CW { get { return MyList.StringToDouble(txt_Ana_CW.Text, 0.0); } set { txt_Ana_CW.Text = value.ToString("f3"); } }
        public double CL { get { return MyList.StringToDouble(txt_Ana_CL.Text, 0.0); } set { txt_Ana_CL.Text = value.ToString("f3"); } }
        public double CR { get { return MyList.StringToDouble(txt_Ana_CR.Text, 0.0); } set { txt_Ana_CR.Text = value.ToString("f3"); } }
        public double Ds { get { return MyList.StringToDouble(txt_Ana_Ds.Text, 0.0); } set { txt_Ana_Ds.Text = value.ToString("f3"); } }
        public double Dso { get { return MyList.StringToDouble(txt_Ana_Dso.Text, 0.0); } set { txt_Ana_Dso.Text = value.ToString("f3"); } }
        public double Y_c_dry { get { return MyList.StringToDouble(txt_Ana_gamma_c_dry.Text, 0.0); } set { txt_Ana_gamma_c_dry.Text = value.ToString("f3"); } }
        public double Y_c_wet { get { return MyList.StringToDouble(txt_Ana_gamma_c_wet.Text, 0.0); } set { txt_Ana_gamma_c_wet.Text = value.ToString("f3"); } }
        public double Ang { get { return MyList.StringToDouble(txt_Ana_ang.Text, 0.0); } set { txt_Ana_ang.Text = value.ToString("f3"); } }
        public double NMG { get { return MyList.StringToDouble(txt_Ana_NMG.Text, 0.0); } set { txt_Ana_NMG.Text = value.ToString("f3"); } }
        public double SMG { get { return MyList.StringToDouble(txt_Ana_SMG.Text, 0.0); } set { txt_Ana_SMG.Text = value.ToString("f3"); } }
        public double DMG { get { return MyList.StringToDouble(txt_Ana_DMG.Text, 0.0); } set { txt_Ana_DMG.Text = value.ToString("f3"); } }
        public double Deff { get { return (DMG - 0.0500 - 0.016 - 6 * 0.032); } }
        public double BMG { get { return MyList.StringToDouble(txt_sec_in_mid_lg_bw.Text, 0.0); } set { txt_sec_in_mid_lg_bw.Text = value.ToString("f3"); } }
        public double NCG { get { return MyList.StringToDouble(txt_Ana_NCG.Text, 0.0); } set { txt_Ana_NCG.Text = value.ToString("f3"); } }
        public double DCG { get { return MyList.StringToDouble(txt_sec_int_cg_d.Text, 0.0); } set { txt_sec_int_cg_d.Text = value.ToString("f3"); } }
        public double BCG { get { return MyList.StringToDouble(txt_sec_int_cg_bw.Text, 0.0); } set { txt_sec_int_cg_bw.Text = value.ToString("f3"); } }
        public double Dw { get { return MyList.StringToDouble(txt_Ana_Dw.Text, 0.0); } set { txt_Ana_Dw.Text = value.ToString("f3"); } }
        public double Y_w { get { return MyList.StringToDouble(txt_Ana_gamma_w.Text, 0.0); } set { txt_Ana_gamma_w.Text = value.ToString("f3"); } }
        public double Hc { get { return MyList.StringToDouble(txt_Ana_Hc.Text, 0.0); } set { txt_Ana_Hc.Text = value.ToString("f3"); } }
        public double Wc { get { return MyList.StringToDouble(txt_Ana_Wc.Text, 0.0); } set { txt_Ana_Wc.Text = value.ToString("f3"); } }
        public double Wf { get { return MyList.StringToDouble(txt_Ana_Wf.Text, 0.0); } set { txt_Ana_Wf.Text = value.ToString("f3"); } }
        public double Hs { get { return MyList.StringToDouble(txt_Ana_Hf.Text, 0.0); } set { txt_Ana_Hf.Text = value.ToString("f3"); } }
        public double Wk { get { return MyList.StringToDouble(txt_Ana_Wk.Text, 0.0); } set { txt_Ana_Wk.Text = value.ToString("f3"); } }
        public double Wr { get { return MyList.StringToDouble(txt_Ana_Wr.Text, 0.0); } set { txt_Ana_Wr.Text = value.ToString("f3"); } }
        //public double swf { get { return MyList.StringToDouble(txt_Ana_swf.Text, 0.0); } set { txt_Ana_swf.Text = value.ToString("f3"); } }
        public double swf { get { return  1.0; } }


        public double og { get { return MyList.StringToDouble(txt_Ana_og.Text, 0.0); } set { txt_Ana_og.Text = value.ToString("f3"); } }
        public double os { get { return MyList.StringToDouble(txt_Ana_os.Text, 0.0); } set { txt_Ana_os.Text = value.ToString("f3"); } }
        public double eg { get { return MyList.StringToDouble(txt_Ana_eg.Text, 0.0); } set { txt_Ana_eg.Text = value.ToString("f3"); } }
        public double Lvp { get { return MyList.StringToDouble(txt_Ana_Lvp.Text, 0.0); } set { txt_Ana_Lvp.Text = value.ToString("f3"); } }
        public double Lsp { get { return MyList.StringToDouble(txt_Ana_Lsp.Text, 0.0); } set { txt_Ana_Lsp.Text = value.ToString("f3"); } }
        public double wgc { get { return MyList.StringToDouble(txt_Ana_wgc.Text, 0.0); } set { txt_Ana_wgc.Text = value.ToString("f3"); } }
        public double wgr { get { return MyList.StringToDouble(txt_Ana_wgr.Text, 0.0); } set { txt_Ana_wgr.Text = value.ToString("f3"); } }
        public double ils { get { return MyList.StringToDouble(txt_Ana_ils.Text, 0.0); } set { txt_Ana_ils.Text = value.ToString("f3"); } }
        public double leff { get { return MyList.StringToDouble(txt_Ana_Leff.Text, 0.0); } set { txt_Ana_Leff.Text = value.ToString("f3"); } }


        #endregion Chiranjit [2012 06 20]

        //Chiranjit [2011 11 04] for Display Results
        public List<string> Result { get; set; }
        public string Result_Report
        {
            get
            {
                return Path.Combine(user_path, "ANALYSIS_RESULT.TXT");
            }
        }

        public string Project_Name 
        { 
            get
            {
                return txt_project_name.Text;
            }
            set
            {
                txt_project_name.Text = value;
            }
        }

        public string user_path
        {
            get
            {
                return iApp.user_path;
            }
            set
            {
                iApp.user_path = value;
            }
        }
        public string Worksheet_Folder
        {
            get
            {
                if (Path.GetFileName(user_path) == Project_Name)
                if (Directory.Exists(Path.Combine(user_path, "Worksheet_Design")) == false)
                    Directory.CreateDirectory(Path.Combine(user_path, "Worksheet_Design"));
                return Path.Combine(user_path, "Worksheet_Design");
            }
        }
        public string Drawing_Folder
        {
            get
            {
                if (Path.GetFileName(user_path) == Project_Name)
                {
                    if (Directory.Exists(Path.Combine(user_path, "DRAWINGS")) == false)
                        Directory.CreateDirectory(Path.Combine(user_path, "DRAWINGS"));
                }
                return Path.Combine(user_path, "DRAWINGS");
            }
        }
        public string Design_Drawing_Folder
        {
            get
            {
                if (Directory.Exists(Path.Combine(Drawing_Folder, "DESIGN DRAWINGS")) == false)
                    Directory.CreateDirectory(Path.Combine(Drawing_Folder, "DESIGN DRAWINGS"));
                return Path.Combine(Drawing_Folder, "DESIGN DRAWINGS");
            }
        }
        public ReadForceType GetForceType()
        {
            ReadForceType rft = new ReadForceType();
            rft.M1 = true;
            rft.R1 = true;
            rft.M2 = chk_M2.Checked;
            rft.M3 = chk_M3.Checked;
            rft.R3 = chk_R3.Checked;
            rft.R2 = chk_R2.Checked;
            return rft;
        }

        #region Form Events
        private void frm_MinorBridge_LS_Load(object sender, EventArgs e)
        {
            Set_Project_Name();

            #region Analysis Data

            Minor_Analysis = new RCC_Minor_LS_Analysis(iApp);

            //Default_Moving_LoadData(dgv_deck_liveloads);
            Default_Moving_LoadData(dgv_long_liveloads);
            Default_Moving_Type_LoadData(dgv_long_loads);
            //Default_Moving_Type_LoadData(dgv_deck_loads);
            Deckslab_User_Input();
            Long_Girder_User_Input();
            Cross_Girder_User_Input();



            cmb_NMG.SelectedIndex = 1;
            //Chiranjit [2014 09 03]
            cmb_HB.SelectedIndex = 0;
            British_Interactive();



            if (iApp.DesignStandard == eDesignStandard.BritishStandard)
            {
                cmb_long_open_file.Items.Clear();
                cmb_long_open_file.Items.Add(string.Format("DEAD LOAD ANALYSIS"));
                cmb_long_open_file.Items.Add(string.Format("LIVE LOAD ANALYSIS"));
                cmb_long_open_file.Items.Add(string.Format("LIVE LOAD ANALYSIS 1"));
                cmb_long_open_file.Items.Add(string.Format("LIVE LOAD ANALYSIS 2"));
                cmb_long_open_file.Items.Add(string.Format("LIVE LOAD ANALYSIS 3"));
                cmb_long_open_file.Items.Add(string.Format("LIVE LOAD ANALYSIS 4"));
                cmb_long_open_file.Items.Add(string.Format("LIVE LOAD ANALYSIS 5"));
                cmb_long_open_file.Items.Add(string.Format("DL + LL COMBINE ANALYSIS"));
                //cmb_long_open_file.Items.Add(string.Format("LONGITUDINAL GIRDER ANALYSIS RESULTS"));

                tabCtrl.TabPages.Remove(tab_mov_data_Indian);
                tc_limit_design.TabPages.Remove(tab_deck_slab_IS);

            }
            else
            {
                tabCtrl.TabPages.Remove(tab_mov_data_british);
                tc_limit_design.TabPages.Remove(tab_deck_slab_BS);
                tab_deck_slab_IS.Text = "Deck Slab";
            }


            #endregion Analysis Data

            #region RCC Abutment
            Abut = new RCC_AbutmentWall(iApp);
            //pic_cantilever.BackgroundImage = AstraFunctionOne.ImageCollection.Abutment;
            //cmb_abut_fck.SelectedIndex = 3;
            //cmb_abut_fy.SelectedIndex = 1;
            #endregion RCC Abutment

            #region RCC Pier
            rcc_pier = new RccPier(iApp);
            cmb_rcc_pier_fck.SelectedIndex = 3;
            cmb_rcc_pier_fy.SelectedIndex = 1;
            cmb_pier_2_k.SelectedIndex = 1;
            #endregion RCC Pier

            #region Deckslab
            uC_Deckslab_BS1.iApp = iApp;
            #endregion Deckslab


            #region IRC Abutment


            //tc_limit_design.TabPages.Remove(tab_abutment);
            uC_RCC_Abut1.iApp = iApp;
            uC_RCC_Abut1.Load_Data();

            #endregion IRC Abutment


            #region Bearings

            //Chiranjit [2016 03 1]
            uC_BRD1.iApp = iApp;
            uC_BRD1.Load_Default_Data();
            iApp.user_path = Path.Combine(iApp.LastDesignWorkingFolder, Title); ;

            #endregion Bearings


            cmb_long_open_file.SelectedIndex = 0;
            //cmb_deck_input_files.SelectedIndex = 0;

            //long_pictureBox1.BackgroundImage = AstraFunctionOne.ImageCollection.T_Beam_Secion_for_Dimensions;
            //long_pictureBox2.BackgroundImage = AstraFunctionOne.ImageCollection.RCC_T_Beam_Bridge;
            Button_Enable_Disable();

            tab_sections.TabPages.Remove(tab_details);

            txt_Ana_B.Text = txt_Ana_B.Text + "";
            Text_Changed();

            //Open_Project();

            #region Hide Long Girders and Cross Girders
            tc_limit_design.TabPages.Remove(tab_long_main_girders);
            tc_limit_design.TabPages.Remove(tab_cross_girders);

            tabCtrl.TabPages.Remove(tab_sec_props);

            tabCtrl.TabPages.Remove(tab_mov_data_Indian);

            tabCtrl.TabPages.Remove(tab_mov_data_british);

            tabCtrl.TabPages.Remove(tab_result);

            #endregion Hide Long Girders and Cross Girders
        }

        private void Open_Project()
        {

            //Chiranjit [2014 10 06]
            #region Chiranjit Design Option

            try
            {
                //eDesignOption edp = iApp.Get_Design_Option(Title);
                //if (edp == eDesignOption.None)
                //{
                //    this.Close();
                //}
                //else if (edp == eDesignOption.Open_Design)
                //{
                IsCreateData = false;
                string chk_file = "";

                //user_path = Path.Combine(iApp.LastDesignWorkingFolder, Title);

                string usp = Path.Combine(user_path, "Long Girder Analysis");
                if (Directory.Exists(usp))
                {
                    chk_file = Path.Combine(usp, "INPUT_DATA.TXT");
                    Minor_Analysis.Input_File = chk_file;
                }

                Ana_OpenAnalysisFile(chk_file);

                uC_BRD1.user_path = user_path;
                uC_BRD1.Set_VBAB_Input_Data();
                uC_BRD1.Set_VFB_Input_Data();
                uC_BRD1.Set_VMABL_Input_Data();
                uC_BRD1.Set_VMABT_Input_Data();

                Read_All_Data();

                #region Read Previous Record
                IsRead = true;
                iApp.Read_Form_Record(this, user_path);
                IsRead = false;
                txt_analysis_file.Text = chk_file;

                if (iApp.DesignStandard == eDesignStandard.BritishStandard)
                {
                    British_Interactive();

                    Default_British_Type_LoadData(dgv_british_loads);
                    uC_Deckslab_BS1.user_path = user_path;
                }
                else
                {

                    uC_Deckslab_IS1.user_path = user_path;

                }


                #endregion

                rbtn_ana_select_analysis_file.Checked = true; //Chiranjit [2013 06 25]
                Open_Create_Data();//Chiranjit [2013 06 25]
                Text_Changed();
                if (iApp.IsDemo)
                    MessageBox.Show("ASTRA USB Dongle not found at any port....\nOpening with default data......", "ASTRA", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("Data Loaded successfully.", "ASTRA", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //}

                Button_Enable_Disable();

                grb_create_input_data.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Input data file Error..");
            }
            #endregion Chiranjit Design Option

        }
        private void btn_WorkSheet_Design_Click(object sender, EventArgs e)
        {
            string excel_file_name = "";
            string copy_path = "";
            Button btn = sender as Button;
             
            //if (btn.Name == btn_WSD_Tee_Girders.Name)
            //{
            //    excel_file_name = Path.Combine(Application.StartupPath, @"DESIGN\TBEAM Bridge\TBEAM Worksheet Design 2\04 TeeGirder Design.xls");
            //}
            //else if (btn.Name == btn_WSD_Open.Name)
            //{
            //    iApp.Open_WorkSheet_Design();
            //    return;
            //}

            copy_path = Path.Combine(user_path, Path.GetFileName(excel_file_name));

            if (File.Exists(excel_file_name))
            {
                iApp.OpenExcelFile(Worksheet_Folder, excel_file_name, "2011ap");
            }
        }
        #endregion Form Events

        #region Bridge Deck Analysis Form Events
       
        private void btn_update_force_Click(object sender, EventArgs e)
        {
            string ana_rep_file = Minor_Analysis.Total_Analysis_Report;
            if (File.Exists(ana_rep_file))
            {
                Minor_Analysis.TotalLoad_Analysis.ForceType = GetForceType();
                Show_Long_Girder_Moment_Shear();

                MessageBox.Show(this, "Force Data Updated.", "ASTRA", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            Button_Enable_Disable();
        }

        public void Open_Create_Data()
        {

            try
            {
                Ana_Initialize_Analysis_InputData();
            }
            catch (Exception ex) { }
        }
        private void btn_Ana_create_data_Click(object sender, EventArgs e)
        {
            //user_path = IsCreateData ? Path.Combine(iApp.LastDesignWorkingFolder, Title) : user_path;

            #region Chiranjit [2016 08 09]  Add Project Name
            if (IsCreateData)
            {
                if (Path.GetFileName(user_path) != Project_Name)
                    Create_Project();
            }




            #endregion Chiranjit [2016 08 09]  Add Project Name

            //int len = user_path.Length;
            if (!Directory.Exists(user_path))
            {
                Directory.CreateDirectory(user_path);
            }
            try
            {


                Write_All_Data(true);

                
                if(iApp.IsDemo)
                {
                    txt_Ana_L.Text = "19.582";
                    txt_Ana_B.Text = "12.0";
                    cmb_NMG.SelectedIndex = 1;
                }

                Ana_Initialize_Analysis_InputData();

                string usp = Path.Combine(user_path, "Long Girder Analysis");

                string inp_file = Path.Combine(user_path, "INPUT_DATA.TXT");


                if (!Directory.Exists(usp)) { Directory.CreateDirectory(usp); }

                //Chiranjit [2014 09 03]
                if (iApp.DesignStandard == eDesignStandard.IndianStandard)
                    LONG_GIRDER_LL_TXT();
                else
                    LONG_GIRDER_BRITISH_LL_TXT();





                uC_Deckslab_IS1.user_path = user_path;


                Minor_Analysis.Input_File = Path.Combine(usp, "INPUT_DATA.TXT");




                Minor_Analysis.Start_Support = Start_Support_Text;
                Minor_Analysis.End_Support = END_Support_Text;

                if (iApp.DesignStandard == eDesignStandard.IndianStandard)
                {
                    Minor_Analysis.CreateData();
                    
                    Minor_Analysis.WriteData_Total_Analysis(Minor_Analysis.Input_File);
                    Minor_Analysis.WriteData_Total_Analysis(inp_file);

                    Calculate_Load_Computation(Minor_Analysis._Outer_Girder_Mid,
                        Minor_Analysis._Inner_Girder_Mid,
                         Minor_Analysis.joints_list_for_load);

                    //Long_Girder_Analysis.WriteData_Total_Analysis(Long_Girder_Analysis.TotalAnalysis_Input_File);

                    Minor_Analysis.WriteData_LiveLoad_Analysis(Minor_Analysis.TotalAnalysis_Input_File, long_ll);

                    //Long_Girder_Analysis.WriteData_LiveLoad_Analysis(Long_Girder_Analysis.LiveLoadAnalysis_Input_File, long_ll);


                    #region Chiranjit [2014 10 2]
                    for (int i = 0; i < all_loads.Count; i++)
                    {
                        Minor_Analysis.WriteData_LiveLoad_Analysis(Minor_Analysis.Get_LL_Analysis_Input_File(i + 1), long_ll);
                    }
                    #endregion Chiranjit [2014 10 2]

                    //Long_Girder_Analysis.WriteData_LiveLoad_Analysis(Long_Girder_Analysis.LL_Analysis_1_Input_File, long_ll);
                    //Long_Girder_Analysis.WriteData_LiveLoad_Analysis(Long_Girder_Analysis.LL_Analysis_2_Input_File, long_ll);
                    //Long_Girder_Analysis.WriteData_LiveLoad_Analysis(Long_Girder_Analysis.LL_Analysis_3_Input_File, long_ll);
                    //Long_Girder_Analysis.WriteData_LiveLoad_Analysis(Long_Girder_Analysis.LL_Analysis_4_Input_File, long_ll);
                    //Long_Girder_Analysis.WriteData_LiveLoad_Analysis(Long_Girder_Analysis.LL_Analysis_5_Input_File, long_ll);
                    //Long_Girder_Analysis.WriteData_LiveLoad_Analysis(Long_Girder_Analysis.LL_Analysis_6_Input_File, long_ll);




                    Minor_Analysis.WriteData_DeadLoad_Analysis(Minor_Analysis.DeadLoadAnalysis_Input_File);

                    Ana_Write_Long_Girder_Load_Data(Minor_Analysis.Input_File, true, true);
                    Ana_Write_Long_Girder_Load_Data(Minor_Analysis.TotalAnalysis_Input_File, true, true);


                    //Ana_Write_Long_Girder_Load_Data(Long_Girder_Analysis.LiveLoadAnalysis_Input_File, true, false);


                    #region Chiranjit [2014 10 22]

                    cmb_long_open_file.Items.Clear();
                    cmb_long_open_file.Items.Add("DEAD LOAD ANALYSIS");
                    //cmb_long_open_file.Items.Add("LIVE LOAD ANALYSIS");




                    for (int i = 0; i < all_loads.Count; i++)
                    {
                        Ana_Write_Long_Girder_Load_Data(Minor_Analysis.Get_LL_Analysis_Input_File(i + 1), true, false, i + 1);

                        if (ll_comb.Count == all_loads.Count)
                        {
                            cmb_long_open_file.Items.Add("LIVE LOAD ANALYSIS (" + ll_comb[i] + ")");

                        }
                        else
                            cmb_long_open_file.Items.Add("LIVE LOAD ANALYSIS " + (i + 1));
                    }
                    cmb_long_open_file.Items.Add("DL + LL ANALYSIS");
                    Ana_Write_Long_Girder_Load_Data(Minor_Analysis.TotalAnalysis_Input_File, true, true, DL_LL_Comb_Load_No);

                    #endregion Chiranjit [2014 10 22]

                    //Chiranjit [2013 09 24] Without Dead Load
                    //Ana_Write_Long_Girder_Load_Data(Long_Girder_Analysis.LL_Analysis_1_Input_File, true, false, 1);
                    //Ana_Write_Long_Girder_Load_Data(Long_Girder_Analysis.LL_Analysis_2_Input_File, true, false, 2);
                    //Ana_Write_Long_Girder_Load_Data(Long_Girder_Analysis.LL_Analysis_3_Input_File, true, false, 3);
                    //Ana_Write_Long_Girder_Load_Data(Long_Girder_Analysis.LL_Analysis_4_Input_File, true, false, 4);
                    //Ana_Write_Long_Girder_Load_Data(Long_Girder_Analysis.LL_Analysis_5_Input_File, true, false, 5);





                    //if (iApp.DesignStandard == eDesignStandard.IndianStandard)
                    //{
                    //    Ana_Write_Long_Girder_Load_Data(Long_Girder_Analysis.LL_Analysis_6_Input_File, true, false, 6);
                    //    Ana_Write_Long_Girder_Load_Data(Long_Girder_Analysis.TotalAnalysis_Input_File, true, true, 7);
                    //}
                    //else
                    //{
                    //    Ana_Write_Long_Girder_Load_Data(Long_Girder_Analysis.TotalAnalysis_Input_File, true, true, 5);
                    //    Ana_Write_Long_Girder_Load_Data(Long_Girder_Analysis.LiveLoadAnalysis_Input_File, true, false, 5);
                    //}
                }
                else
                {
                    uC_Deckslab_BS1.user_path = user_path;

                    Minor_Analysis.HA_Lanes = HA_Lanes;

                    Minor_Analysis.CreateData_British();

                    Minor_Analysis.WriteData_Total_Analysis(Minor_Analysis.Input_File, true);
                    Minor_Analysis.WriteData_Total_Analysis(inp_file, true);

                    Calculate_Load_Computation(Minor_Analysis._Outer_Girder_Mid,
                        Minor_Analysis._Inner_Girder_Mid,
                         Minor_Analysis.joints_list_for_load);

                    //Long_Girder_Analysis.WriteData_Total_Analysis(Long_Girder_Analysis.TotalAnalysis_Input_File);

                    Minor_Analysis.WriteData_LiveLoad_Analysis(Minor_Analysis.TotalAnalysis_Input_File, long_ll);
                    Minor_Analysis.WriteData_LiveLoad_Analysis(Minor_Analysis.LiveLoadAnalysis_Input_File, long_ll);
                    Minor_Analysis.WriteData_DeadLoad_Analysis(Minor_Analysis.DeadLoadAnalysis_Input_File);

                    Ana_Write_Long_Girder_Load_Data(Minor_Analysis.Input_File, true, true);


                    if (rbtn_HA.Checked == false)
                    {
                        Minor_Analysis.WriteData_LiveLoad_Analysis(Minor_Analysis.LL_Analysis_1_Input_File, long_ll);
                        Minor_Analysis.WriteData_LiveLoad_Analysis(Minor_Analysis.LL_Analysis_2_Input_File, long_ll);
                        Minor_Analysis.WriteData_LiveLoad_Analysis(Minor_Analysis.LL_Analysis_3_Input_File, long_ll);
                        Minor_Analysis.WriteData_LiveLoad_Analysis(Minor_Analysis.LL_Analysis_4_Input_File, long_ll);
                        Minor_Analysis.WriteData_LiveLoad_Analysis(Minor_Analysis.LL_Analysis_5_Input_File, long_ll);


                        Ana_Write_Long_Girder_Load_Data(Minor_Analysis.TotalAnalysis_Input_File, true, true, 5);
                        Ana_Write_Long_Girder_Load_Data(Minor_Analysis.LiveLoadAnalysis_Input_File, true, false, 5);

                        //Chiranjit [2013 09 24] Without Dead Load
                        Ana_Write_Long_Girder_Load_Data(Minor_Analysis.LL_Analysis_1_Input_File, true, false, 1);
                        Ana_Write_Long_Girder_Load_Data(Minor_Analysis.LL_Analysis_2_Input_File, true, false, 2);
                        Ana_Write_Long_Girder_Load_Data(Minor_Analysis.LL_Analysis_3_Input_File, true, false, 3);
                        Ana_Write_Long_Girder_Load_Data(Minor_Analysis.LL_Analysis_4_Input_File, true, false, 4);
                        Ana_Write_Long_Girder_Load_Data(Minor_Analysis.LL_Analysis_5_Input_File, true, false, 5);


                    }
                    else
                    {
                        Ana_Write_Long_Girder_Load_Data(Minor_Analysis.TotalAnalysis_Input_File, true, true, 1);
                        Ana_Write_Long_Girder_Load_Data(Minor_Analysis.LiveLoadAnalysis_Input_File, false, false, 1);
                    }

                }


                Ana_Write_Long_Girder_Load_Data(Minor_Analysis.DeadLoadAnalysis_Input_File, false, true);

                Minor_Analysis.TotalLoad_Analysis = new BridgeMemberAnalysis(iApp, Minor_Analysis.TotalAnalysis_Input_File);

                string ll_txt = Minor_Analysis.LiveLoad_File;

                Minor_Analysis.Live_Load_List = LoadData.GetLiveLoads(ll_txt);

                if (Minor_Analysis.Live_Load_List == null) return;

                cmb_long_open_file.SelectedIndex = 0;
                Button_Enable_Disable();

                MessageBox.Show(this, "Analysis Input data is created as \"" + Project_Name + "\\INPUT_DATA.TXT\" inside the working folder.",
                  "ASTRA", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex) 
            { 
                //Long_Girder_Analysis.Input_File.Length
                MessageBox.Show(ex.Message, "ASTRA", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void btn_Ana_close_Click(object sender, EventArgs e)
        {
            Minor_Analysis.Clear();

            this.Close();
        } 
        private void btn_Ana_view_data_Click(object sender, EventArgs e)
        {
            string file_name = "";
            string ll_txt = "";

            Button btn = sender as Button;

            #region Set File Name
            file_name = Get_LongGirder_File(cmb_long_open_file.SelectedIndex);

            #endregion Set File Name

            ll_txt = MyList.Get_LL_TXT_File(file_name);
            if (btn.Name == btn_view_data.Name)
            {

                iApp.View_Input_File(file_name);

                //if (File.Exists(ll_txt))
                //    iApp.RunExe(ll_txt);
                //if (File.Exists(file_name))
                //    iApp.RunExe(file_name);
            }
            else if (btn.Name == btn_view_structure.Name)
            {
                if (File.Exists(file_name))
                    iApp.OpenWork(file_name, false);
            }
            else if (btn.Name == btn_view_report.Name)
            {
                file_name = MyList.Get_Analysis_Report_File(file_name);
                if (File.Exists(file_name))
                    iApp.RunExe(file_name);
            }
            else if (btn.Name == btn_View_Moving_Load.Name)
            {

                //file_name = MyList.Get_Analysis_Report_File(file_name);
                if (File.Exists(MyList.Get_Analysis_Report_File(file_name)))
                    iApp.OpenWork(file_name, true);
            }
        }

        private string Get_LongGirder_File(int index)
        {
            string file_name = "" ;
            
            if (iApp.DesignStandard == eDesignStandard.IndianStandard)
            {
                if (index == 0)
                {
                    file_name = Minor_Analysis.DeadLoadAnalysis_Input_File;
                }
                else if (index == all_loads.Count + 1)
                {
                    file_name = Minor_Analysis.TotalAnalysis_Input_File;
                }
                else 
                {
                    file_name = Minor_Analysis.Get_LL_Analysis_Input_File(index);
                }
            }
            if (iApp.DesignStandard == eDesignStandard.BritishStandard)
            {
                if (index== -1) return "";
                string item = cmb_long_open_file.Items[index].ToString();

                if (item.StartsWith("DL + LL") || item.StartsWith("TOTAL"))
                {
                    file_name = Minor_Analysis.TotalAnalysis_Input_File;
                }
                else if (item.StartsWith("DEAD LOAD"))
                {
                    file_name = Minor_Analysis.DeadLoadAnalysis_Input_File;
                }
                else if (item.StartsWith("LIVE LOAD ANALYSIS 1"))
                {
                    file_name = Minor_Analysis.LL_Analysis_1_Input_File;
                }
                else if (item.StartsWith("LIVE LOAD ANALYSIS 2"))
                {
                    file_name = Minor_Analysis.LL_Analysis_2_Input_File;
                }
                else if (item.StartsWith("LIVE LOAD ANALYSIS 3"))
                {
                    file_name = Minor_Analysis.LL_Analysis_3_Input_File;
                }
                else if (item.StartsWith("LIVE LOAD ANALYSIS 4"))
                {
                    file_name = Minor_Analysis.LL_Analysis_4_Input_File;
                }
                else if (item.StartsWith("LIVE LOAD ANALYSIS 5"))
                {
                    file_name = Minor_Analysis.LL_Analysis_5_Input_File;
                }
                else if (item.StartsWith("LIVE LOAD ANALYSIS"))
                {
                    file_name = Minor_Analysis.LiveLoadAnalysis_Input_File;
                }
                else if (item.StartsWith("LONGITUDINAL GIRDER ANALYSIS RESULTS"))
                {
                    file_name = File_Long_Girder_Results;
                }
            }
            return file_name;
        }
        private void btn_Ana_process_analysis_Click(object sender, EventArgs e)
        {
            try
            {
                #region Process
                int i = 0;
                Write_All_Data(true);


                ProcessCollection pcol = new ProcessCollection();

                ProcessData pd = new ProcessData();

                string flPath = Minor_Analysis.Input_File;
                iApp.Progress_Works.Clear();
                do
                {
                    flPath = Get_LongGirder_File(i);

                    if (File.Exists(flPath))
                    {
                        pd = new ProcessData();
                        pd.Process_File_Name = flPath;
                        pd.Process_Text = "PROCESS ANALYSIS FOR " + Path.GetFileNameWithoutExtension(flPath).ToUpper();
                        pcol.Add(pd);
                        iApp.Progress_Works.Add("Reading Analysis Data from " + Path.GetFileNameWithoutExtension(flPath).ToUpper() + " (ANALYSIS_REP.TXT)");
                    }


                    i++;
                }
                while (i < ((iApp.DesignStandard ==  eDesignStandard.IndianStandard) ? (all_loads.Count + 2) : (all_loads.Count + 3)));
                //while (i < 3) ;




                //frm_LS_Process ff = new frm_LS_Process(pcol);
                //ff.Owner = this;
                ////MessageBox.Show(ff.ShowDialog().ToString());
                //if (ff.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;



                //string ana_rep_file = Long_Girder_Analysis.Analysis_Report;
                string ana_rep_file = Minor_Analysis.Total_Analysis_Report;
                if (iApp.Show_and_Run_Process_List(pcol))
                {
                    //iApp.Progress_Works.Clear();
                    //iApp.Progress_Works.Add("Reading Analysis Data from Total Load Analysis Report File (ANALYSIS_REP.TXT)");
                    //iApp.Progress_Works.Add("Reading Analysis Data from Live Load Analysis Report File (ANALYSIS_REP.TXT)");
                    //iApp.Progress_Works.Add("Reading Analysis Data from Live Load Analysis 1 Report File (ANALYSIS_REP.TXT)");
                    //iApp.Progress_Works.Add("Reading Analysis Data from Live Load Analysis 2 Report File (ANALYSIS_REP.TXT)");
                    //iApp.Progress_Works.Add("Reading Analysis Data from Live Load Analysis 3 Report File (ANALYSIS_REP.TXT)");
                    //iApp.Progress_Works.Add("Reading Analysis Data from Live Load Analysis 4 Report File (ANALYSIS_REP.TXT)");
                    //iApp.Progress_Works.Add("Reading Analysis Data from Live Load Analysis 5 Report File (ANALYSIS_REP.TXT)");

                    //if (iApp.DesignStandard == eDesignStandard.IndianStandard)
                    //    iApp.Progress_Works.Add("Reading Analysis Data from Live Load Analysis 6 Report File (ANALYSIS_REP.TXT)");
                    //iApp.Progress_Works.Add("Reading Analysis Data from Dead Load Analysis Report File (ANALYSIS_REP.TXT)");

                    Minor_Analysis.TotalLoad_Analysis = null;


                    //Long_Girder_Analysis.LiveLoad_Analysis = new BridgeMemberAnalysis(iApp, Long_Girder_Analysis.LiveLoad_Analysis_Report);


                    Minor_Analysis.DeadLoad_Analysis = new BridgeMemberAnalysis(iApp, Minor_Analysis.DeadLoad_Analysis_Report);

                    if (rbtn_HA.Checked == false)
                    {
                        Minor_Analysis.All_LL_Analysis = new List<BridgeMemberAnalysis>();
                        for (i = 0; i < all_loads.Count; i++)
                        {
                            string fn = MyList.Get_Analysis_Report_File(Minor_Analysis.Get_LL_Analysis_Input_File(i + 1));
                            if (File.Exists(fn))
                            {
                                Minor_Analysis.All_LL_Analysis.Add(new BridgeMemberAnalysis(iApp, fn));
                            }
                        }

                        //Long_Girder_Analysis.LiveLoad_1_Analysis = new BridgeMemberAnalysis(iApp,
                        //        Long_Girder_Analysis.Get_Analysis_Report_File(Long_Girder_Analysis.LL_Analysis_1_Input_File));
                        
                        //Long_Girder_Analysis.LiveLoad_2_Analysis = new BridgeMemberAnalysis(iApp,
                        //    Long_Girder_Analysis.Get_Analysis_Report_File(Long_Girder_Analysis.LL_Analysis_2_Input_File));

                        //Long_Girder_Analysis.LiveLoad_3_Analysis = new BridgeMemberAnalysis(iApp,
                        //    Long_Girder_Analysis.Get_Analysis_Report_File(Long_Girder_Analysis.LL_Analysis_3_Input_File));

                        //Long_Girder_Analysis.LiveLoad_4_Analysis = new BridgeMemberAnalysis(iApp,
                        //    Long_Girder_Analysis.Get_Analysis_Report_File(Long_Girder_Analysis.LL_Analysis_4_Input_File));


                        //Long_Girder_Analysis.LiveLoad_5_Analysis = new BridgeMemberAnalysis(iApp,
                        //    Long_Girder_Analysis.Get_Analysis_Report_File(Long_Girder_Analysis.LL_Analysis_5_Input_File));
                    }
                    Minor_Analysis.TotalLoad_Analysis = new BridgeMemberAnalysis(iApp, Minor_Analysis.Total_Analysis_Report);

                    //if (iApp.DesignStandard == eDesignStandard.IndianStandard)
                    //    Long_Girder_Analysis.LiveLoad_6_Analysis = new BridgeMemberAnalysis(iApp,
                    //    Long_Girder_Analysis.Get_Analysis_Report_File(Long_Girder_Analysis.LL_Analysis_6_Input_File));

                    Minor_Analysis.LiveLoad_Analysis = Minor_Analysis.All_LL_Analysis[DL_LL_Comb_Load_No - 1];

                    if (!iApp.Is_Progress_Cancel)
                    {
                        //Show_Long_Girder_Moment_Shear();
                        if (iApp.DesignStandard == eDesignStandard.IndianStandard)
                        {
                            Show_Long_Girder_Moment_Shear();
                        }
                        else
                        {
                            Minor_Analysis.LiveLoad_1_Analysis = Minor_Analysis.All_LL_Analysis[0];
                            Minor_Analysis.LiveLoad_2_Analysis = Minor_Analysis.All_LL_Analysis[1];
                            Minor_Analysis.LiveLoad_3_Analysis = Minor_Analysis.All_LL_Analysis[2];
                            Minor_Analysis.LiveLoad_4_Analysis = Minor_Analysis.All_LL_Analysis[3];
                            Minor_Analysis.LiveLoad_5_Analysis = Minor_Analysis.All_LL_Analysis[4];
                            Show_British_Long_Girder_Moment_Shear();
                        }
                    }
                    else
                    {
                        iApp.Progress_Works.Clear();
                        iApp.Progress_OFF();
                        return;
                    }

                    #region Abutment & Pier
                    string s1 = "";
                    string s2 = "";
                    try
                    {
                        for (i = 0; i < Minor_Analysis.TotalLoad_Analysis.Supports.Count; i++)
                        {
                            if (i < Minor_Analysis.TotalLoad_Analysis.Supports.Count / 2)
                            {
                                if (i == Minor_Analysis.TotalLoad_Analysis.Supports.Count / 2 - 1)
                                {
                                    s1 += Minor_Analysis.TotalLoad_Analysis.Supports[i].NodeNo;
                                }
                                else
                                    s1 += Minor_Analysis.TotalLoad_Analysis.Supports[i].NodeNo + ",";
                            }
                            else
                            {
                                if (i == Minor_Analysis.TotalLoad_Analysis.Supports.Count - 1)
                                {
                                    s2 += Minor_Analysis.TotalLoad_Analysis.Supports[i].NodeNo;
                                }
                                else
                                    s2 += Minor_Analysis.TotalLoad_Analysis.Supports[i].NodeNo + ", ";
                            }
                        }
                    }
                    catch (Exception ex) { }
                    double BB = MyList.StringToDouble(txt_Ana_B.Text, 8.5);

                    //Chiranjit [2013 06 28]
                    //txt_node_displace.Text = Long_Girder_Analysis.TotalLoad_Analysis.Node_Displacements.Get_Max_Deflection().ToString();


                    frm_ViewForces(BB, Minor_Analysis.DeadLoad_Analysis_Report, MyList.Get_Analysis_Report_File(Minor_Analysis.Get_LL_Analysis_Input_File(DL_LL_Comb_Load_No)), (s1 + " " + s2));
                    frm_ViewForces_Load();

                    frm_Pier_ViewDesign_Forces(Minor_Analysis.Total_Analysis_Report, s1, s2);
                    frm_ViewDesign_Forces_Load();


                    //frm_ViewForces f = new frm_ViewForces(iApp, BB, Long_Girder_Analysis.DeadLoad_Analysis_Report, Long_Girder_Analysis.LiveLoad_Analysis_Report, s);
                    //f.Owner = this;
                    //f.Text = "Data to be used in RCC Abutment Design";
                    //f.Show();


                    //frm_Pier_ViewDesign_Forces fv = new frm_Pier_ViewDesign_Forces(iApp, Long_Girder_Analysis.Total_Analysis_Report, s, s1);
                    //fv.Owner = this;
                    //fv.Text = "Data to be used in RCC Pier Design";
                    //fv.Show();

                    //Chiranjit [2012 06 22]
                    txt_ana_DLSR.Text = Total_DeadLoad_Reaction;
                    txt_ana_LLSR.Text = Total_LiveLoad_Reaction;

                    txt_ana_TSRP.Text = txt_final_vert_rec_kN.Text;
                    txt_ana_MSLD.Text = txt_max_Mx_kN.Text;
                    txt_ana_MSTD.Text = txt_max_Mz_kN.Text;



                    txt_RCC_Pier_W1_supp_reac.Text = txt_final_vert_rec_kN.Text;
                    txt_RCC_Pier_Mx1.Text = txt_max_Mx_kN.Text;
                    txt_RCC_Pier_Mz1.Text = txt_max_Mz_kN.Text;

                    //txt_abut_w6.Text = Total_LiveLoad_Reaction;
                    //txt_pier_2_P3.Text = Total_LiveLoad_Reaction;
                    //txt_abut_w6.ForeColor = Color.Red;

                    //txt_abut_w5.Text = Total_DeadLoad_Reaction;
                    //txt_pier_2_P2.Text = Total_DeadLoad_Reaction;
                    //txt_abut_w5.ForeColor = Color.Red;

                    #endregion Abutment & Pier
                }

                //grb_create_input_data.Enabled = rbtn_ana_create_analysis_file.Checked;
                grb_select_analysis.Enabled = !rbtn_ana_create_analysis_file.Checked;

                //grb_create_input_data.Enabled = !rbtn_ana_select_analysis_file.Checked;
                grb_select_analysis.Enabled = rbtn_ana_select_analysis_file.Checked;




                iApp.Progress_Works.Clear();
                iApp.Progress_OFF();

                Button_Enable_Disable();
                Write_All_Data(false);


                #endregion Process
            }
            catch (Exception ex) { }
        }
        
        private void rbtn_Ana_select_analysis_file_CheckedChanged(object sender, EventArgs e)
        {
            //grb_create_input_data.Enabled = false;
            grb_select_analysis.Enabled = rbtn_ana_select_analysis_file.Checked;
            //btn_create_data.Enabled = rbtn_ana_create_analysis_file.Checked;

            //if (rbtn_ana_select_analysis_file.Checked)
            //{

            //    string chk_file = Path.Combine(Analysis_Path, "INPUT_DATA.TXT");

            //    if (File.Exists(chk_file))
            //    {
            //        Show_ReadMemberLoad(chk_file);
            //        Ana_OpenAnalysisFile(chk_file);
            //        Read_All_Data();
            //        iApp.LiveLoads.Fill_Combo(ref cmb_load_type);
            //        #region Read Previous Record
            //        //Chiranjit [2013 01 03]
            //        iApp.Read_Form_Record(this, Analysis_Path);
            //    }
            //    #endregion

            //}
            Button_Enable_Disable();


        }
        public string Analysis_Path
        {
            get
            {
                if (Directory.Exists(Path.Combine(iApp.LastDesignWorkingFolder, Title)))
                    return Path.Combine(iApp.LastDesignWorkingFolder, Title);
                return iApp.LastDesignWorkingFolder;
            }
        }

        private void btn_Ana_browse_input_file_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Text File (*.txt)|*.txt";
                    ofd.InitialDirectory = Analysis_Path;
                    if (ofd.ShowDialog() != DialogResult.Cancel)
                    {
                        IsCreateData = false;
                        string chk_file = "";
                        user_path = Path.GetDirectoryName(ofd.FileName);

                        string usp = Path.Combine(user_path, "Long Girder Analysis");
                        if (Directory.Exists(usp))
                        {
                            chk_file = Path.Combine(usp, "INPUT_DATA.TXT");
                            Minor_Analysis.Input_File = chk_file;
                        }

                        Ana_OpenAnalysisFile(chk_file);
                        Read_All_Data();

                        #region Read Previous Record
                        iApp.Read_Form_Record(this, user_path);
                        txt_analysis_file.Text = chk_file;
                        #endregion

                        rbtn_ana_select_analysis_file.Checked = true; //Chiranjit [2013 06 25]
                        Open_Create_Data();//Chiranjit [2013 06 25]
                        Text_Changed();
                        MessageBox.Show("Data Loaded successfully.", "ASTRA", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }
                }
                Button_Enable_Disable();

                grb_create_input_data.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Input data file Error..");
            }
        }
        
        private void txt_Ana_length_TextChanged(object sender, EventArgs e)
        {
            try
            {
                //txt_Ana_X.Text = "-" + txt_Ana_L.Text; //Chiranjit [2013 05 29]
                Text_Changed();
            }
            catch (Exception ex) { }
        }
        private void chk_Ana_CheckedChanged(object sender, EventArgs e)
        {
            grb_SIDL.Enabled = chk_ana_active_SIDL.Checked;
        }
        #endregion Bridge Deck Analysis Form Events

        #region Bridge Deck Analysis Methods

        void Ana_Initialize_Analysis_InputData()
        {
            if (Minor_Analysis == null)
                Minor_Analysis = new RCC_Minor_LS_Analysis(iApp);


            double Bs = (B - CL - CR) / (NMG - 1);

             
            //Long_Girder_Analysis.Long_Inner_Mid_Section = long_inner_sec;
            //Long_Girder_Analysis.Long_Outer_Mid_Section = long_out_sec;
            //Long_Girder_Analysis.T_Cross_Section = cross_sec;




            Minor_Analysis.Long_Inner_Mid_Section = LG_INNER_MID;
            Minor_Analysis.Long_Outer_Mid_Section = LG_OUTER_MID;

            Minor_Analysis.Long_Inner_Support_Section = LG_INNER_SUP;
            Minor_Analysis.Long_Outer_Support_Section = LG_OUTER_SUP;

            Minor_Analysis.Cross_End_Section = CG_END;
            Minor_Analysis.Cross_Intermediate_Section = CG_INTER;



            Minor_Analysis.Length = MyList.StringToDouble(txt_Ana_L.Text, 0.0);
            Minor_Analysis.WidthBridge = MyList.StringToDouble(txt_Ana_B.Text, 0.0);
            Minor_Analysis.Width_LeftCantilever = MyList.StringToDouble(txt_Ana_CL.Text, 0.0);
            Minor_Analysis.Width_RightCantilever = MyList.StringToDouble(txt_Ana_CR.Text, 0.0);
            Minor_Analysis.Skew_Angle = MyList.StringToDouble(txt_Ana_ang.Text, 0.0);
            Minor_Analysis.NMG = MyList.StringToInt(txt_Ana_NMG.Text, 4);
            Minor_Analysis.NCG = MyList.StringToInt(txt_Ana_NCG.Text, 3);

            Minor_Analysis.Ds = Ds;
            Minor_Analysis.Lvp = Lvp;
            Minor_Analysis.Lsp = Lsp;
            Minor_Analysis.Leff = leff;

            Minor_Analysis.Wc = Wc;
            Minor_Analysis.og = og;
            Minor_Analysis.os = os;

            if (rbtn_footpath.Checked)
            {
                if (chk_fp_left.Checked && !chk_fp_right.Checked)
                {
                    Minor_Analysis.Wf_left = Wf;
                    Minor_Analysis.Wk_left = Wk;

                    Minor_Analysis.Wf_right = 0.0;
                    Minor_Analysis.Wk_right = Wk;
                }
                else if (!chk_fp_left.Checked && chk_fp_right.Checked)
                {
                    Minor_Analysis.Wf_left = 0.0;
                    Minor_Analysis.Wk_left = Wk;

                    Minor_Analysis.Wf_right = Wf;
                    Minor_Analysis.Wk_right = Wk;
                }
                else 
                {
                    Minor_Analysis.Wf_left = Wf;
                    Minor_Analysis.Wk_left = Wk;

                    Minor_Analysis.Wf_right = Wf;
                    Minor_Analysis.Wk_right = Wk;
                }

                Minor_Analysis.Wr = Wr;
            }


            if (Deck_Analysis == null)
                Deck_Analysis = new LS_DeckSlab_Analysis(iApp);

            Deck_Analysis.T_Long_Inner_Section = long_inner_sec;
            Deck_Analysis.T_Long_Outer_Section = long_out_sec;
            Deck_Analysis.T_Cross_Section = cross_sec;

            //Deck_Analysis.Length = MyList.StringToDouble(txt_Ana_B.Text, 0.0);
            //Deck_Analysis.Length = MyList.StringToDouble(dgv_deck_user_input[1,4].Value.ToString(), 0.0);
            //Deck_Analysis.WidthBridge = 6.0;

            Deck_Analysis.Width_LeftCantilever = MyList.StringToDouble(txt_Ana_CL.Text, 0.0);
            Deck_Analysis.Width_RightCantilever = MyList.StringToDouble(txt_Ana_CR.Text, 0.0);
            //Deck_Analysis.Skew_Angle = MyList.StringToDouble(dgv_deck_user_input[1, 5].Value.ToString(), 0.0);
            Deck_Analysis.Number_Of_Long_Girder = MyList.StringToInt(txt_Ana_NMG.Text, 4);
            Deck_Analysis.Number_Of_Cross_Girder = MyList.StringToInt(txt_Ana_NCG.Text, 3);
            Deck_Analysis.WidthBridge = L / (Deck_Analysis.Number_Of_Cross_Girder - 1);

            Deck_Analysis.Lwv = MyList.StringToDouble(txt_Ana_Lvp.Text, 0.0);
            Deck_Analysis.Wkerb = MyList.StringToDouble(txt_Ana_Wc.Text, 0.0);


        }
        void Ana_Write_Long_Girder_Load_Data(string file_name, bool add_LiveLoad, bool add_DeadLoad)
        {
            //string file_name = Bridge_Analysis.Input_File;
            //= Bridge_Analysis.TotalAnalysis_Input_File;
            if (!File.Exists(file_name)) return;

            List<string> inp_file_cont = new List<string>(File.ReadAllLines(file_name));
            string kStr = "";
            int indx = -1;
            bool flag = false;
            MyList mlist = null;
            int i = 0;

            bool isMoving_load = false;
            for (i = 0; i < inp_file_cont.Count; i++)
            {
                kStr = MyList.RemoveAllSpaces(inp_file_cont[i].ToUpper());
                mlist = new MyList(kStr, ' ');

                if (kStr.Contains("LOAD GEN"))
                    isMoving_load = true;

                if (mlist.StringList[0].StartsWith("LOAD") && flag == false)
                {
                    if (indx == -1)
                        indx = i;
                    flag = true;
                }
                if (kStr.Contains("ANALYSIS") || kStr.Contains("PRINT"))
                {
                    flag = false;
                }
                if (flag)
                {
                    inp_file_cont.RemoveAt(i);
                    i--;
                }

            }

            List<string> load_lst = new List<string>();

            string s = " DL";
            bool fl = false;

            if (add_DeadLoad)
            {

                foreach (var item in txt_member_load.Lines)
                {
                    if (add_LiveLoad)
                    {
                        if (item.ToUpper().StartsWith("LOAD"))
                        {
                            if (fl == false)
                            {
                                fl = true;
                                load_lst.Add(item);
                            }
                            else
                                load_lst.Add("*" + item);
                        }
                        else
                        {
                            if (!load_lst.Contains(item))
                                load_lst.Add(item);
                            else
                                load_lst.Add("*" + item);
                        }
                    }
                    else
                        load_lst.Add(item);

                }

                
            }
            else
            {
                //load_lst.Add("LOAD 1 DEAD LOAD");
                //load_lst.Add("MEMBER LOAD");
                //load_lst.Add("1 TO " + Long_Girder_Analysis.MemColls.Count + " UNI GY -0.0001");
            }

            //Bridge_Analysis.LoadReadFromGrid(dgv_live_load);

            //Bridge_Analysis.Live_Load_List = iApp.LiveLoads;
            Minor_Analysis.Live_Load_List = LoadData.GetLiveLoads(Path.Combine(Path.GetDirectoryName(file_name), "ll.txt"));
            if (add_LiveLoad)
            {
                //Chiranjit [2013 10 07]
                //if (dgv_live_load.RowCount != 0)
                //load_lst.AddRange(Ana_Get_MovingLoad_Data(Long_Girder_Analysis.Live_Load_List));
                load_lst.Add("DEFINE MOVING LOAD FILE LL.TXT");
                //load_lst.Add("LOAD GENERATION " + txt_LL_load_gen.Text);
                load_lst.AddRange(load_total_7.ToArray());

            }
            inp_file_cont.InsertRange(indx, load_lst);
            //inp_file_cont.InsertRange(indx, );
            File.WriteAllLines(file_name, inp_file_cont.ToArray());
            //MessageBox.Show(this, "Load data is added in file " + file_name, "ASTRA", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }
        void Ana_Write_Long_Girder_Load_Data(string file_name, bool add_LiveLoad, bool add_DeadLoad, int load_no)
        {
            //string file_name = Bridge_Analysis.Input_File;
            //= Bridge_Analysis.TotalAnalysis_Input_File;
            if (!File.Exists(file_name)) return;

            List<string> inp_file_cont = new List<string>(File.ReadAllLines(file_name));
            string kStr = "";
            int indx = -1;
            bool flag = false;
            MyList mlist = null;
            int i = 0;

            bool isMoving_load = false;
            for (i = 0; i < inp_file_cont.Count; i++)
            {
                kStr = MyList.RemoveAllSpaces(inp_file_cont[i].ToUpper());
                mlist = new MyList(kStr, ' ');

                if (kStr.Contains("LOAD GEN"))
                    isMoving_load = true;

                if (mlist.StringList[0].StartsWith("LOAD") && flag == false)
                {
                    if (indx == -1)
                        indx = i;
                    flag = true;
                }
                if (kStr.Contains("ANALYSIS") || kStr.Contains("PRINT"))
                {
                    flag = false;
                }
                if (flag)
                {
                    inp_file_cont.RemoveAt(i);
                    i--;
                }

            }

            List<string> load_lst = new List<string>();

            string s = " DL";
            bool fl = false;
            if (add_DeadLoad)
            {

                if (add_LiveLoad)
                {
                    foreach (var item in txt_member_load.Lines)
                    {

                        if (item.ToUpper().StartsWith("LOAD"))
                        {
                            if (fl == false)
                            {
                                fl = true;
                                load_lst.Add(item);
                            }
                            else
                                load_lst.Add("*" + item);
                        }
                        else
                        {
                            if (!load_lst.Contains(item))
                                load_lst.Add(item);
                            else
                                load_lst.Add("*" + item);
                        }
                    }
                    if (iApp.DesignStandard == eDesignStandard.BritishStandard)
                    {
                        if (HA_Lanes.Count > 0)
                        {
                            //load_lst.Add("LOAD 1 HA LOADINGS AS PER [BS 5400, Part 2, BD 37/01]");
                            load_lst.Add("*HA LOADINGS AS PER [BS 5400, Part 2, BD 37/01]");
                            load_lst.Add("MEMBER LOAD");
                            //if (chk_self_british.Checked)
                            //    load_lst.Add("SELFWEIGHT Y -1");

                            load_lst.Add(string.Format("{0} UNI GY -{1}", Minor_Analysis.HA_Loading_Members, txt_HA_UDL.Text));

                            //load_lst.Add(string.Format("{0} CON GZ -{1} 0.5", Bridge_Analysis.HA_Loading_Members, txt_HA_CON.Text));


                            foreach (var item in MyList.Get_Array_Intiger(Minor_Analysis.HA_Loading_Members))
                            {
                                load_lst.Add(string.Format("{0} CON GY -{1} {2:f3}", item, txt_HA_CON.Text, Minor_Analysis.MemColls.Get_Member_Length(item.ToString()) / 2));
                            }
                        }
                    }

                }
                else
                    load_lst.AddRange(txt_member_load.Lines);
            }
            else
            {

                if (iApp.DesignStandard == eDesignStandard.IndianStandard)
                {
                    //if (chk_self_indian.Checked)
                    //    load_lst.Add("SELFWEIGHT Y -1");
                }
                else if (iApp.DesignStandard == eDesignStandard.BritishStandard)
                {
                    if (HA_Lanes.Count > 0)
                    {
                        load_lst.Add("LOAD 1 HA LOADINGS AS PER [BS 5400, Part 2, BD 37/01]");
                        load_lst.Add("MEMBER LOAD");
                        //if (chk_self_british.Checked)
                        //    load_lst.Add("SELFWEIGHT Y -1");

                        load_lst.Add(string.Format("{0} UNI GY -{1}", Minor_Analysis.HA_Loading_Members, txt_HA_UDL.Text));

                        //load_lst.Add(string.Format("{0} CON GZ -{1} 0.5", Bridge_Analysis.HA_Loading_Members, txt_HA_CON.Text));


                        foreach (var item in MyList.Get_Array_Intiger(Minor_Analysis.HA_Loading_Members))
                        {
                            load_lst.Add(string.Format("{0} CON GY -{1} {2:f3}", item, txt_HA_CON.Text, Minor_Analysis.MemColls.Get_Member_Length(item.ToString()) / 2));
                        }
                    }
                }
            }

            //Bridge_Analysis.LoadReadFromGrid(dgv_live_load);

            //Bridge_Analysis.Live_Load_List = iApp.LiveLoads;
            Minor_Analysis.Live_Load_List = LoadData.GetLiveLoads(Path.Combine(Path.GetDirectoryName(file_name), "ll.txt"));
            if (add_LiveLoad)
            {

                if ((rbtn_HB.Checked || rbtn_HA_HB.Checked)
                    || iApp.DesignStandard == eDesignStandard.IndianStandard)
                {
                    load_lst.Add("DEFINE MOVING LOAD FILE LL.TXT");

                    load_lst.AddRange(all_loads[load_no - 1].ToArray());

                }

            }
            inp_file_cont.InsertRange(indx, load_lst);
            //inp_file_cont.InsertRange(indx, );
            File.WriteAllLines(file_name, inp_file_cont.ToArray());
            //MessageBox.Show(this, "Load data is added in file " + file_name, "ASTRA", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }
        void Ana_Write_DeckSlab_Load_Data(string file_name, bool add_LiveLoad, bool add_DeadLoad, int load_no)
        {
            //string file_name = Bridge_Analysis.Input_File;
            //= Bridge_Analysis.TotalAnalysis_Input_File;
            if (!File.Exists(file_name)) return;

            List<string> inp_file_cont = new List<string>(File.ReadAllLines(file_name));
            string kStr = "";
            int indx = -1;
            bool flag = false;
            MyList mlist = null;
            int i = 0;

            bool isMoving_load = false;
            for (i = 0; i < inp_file_cont.Count; i++)
            {
                kStr = MyList.RemoveAllSpaces(inp_file_cont[i].ToUpper());
                mlist = new MyList(kStr, ' ');

                if (kStr.Contains("LOAD GEN"))
                    isMoving_load = true;

                if (mlist.StringList[0].StartsWith("LOAD") && flag == false)
                {
                    if (indx == -1)
                        indx = i;
                    flag = true;
                }
                if (kStr.Contains("ANALYSIS") || kStr.Contains("PRINT"))
                {
                    flag = false;
                }
                if (flag)
                {
                    inp_file_cont.RemoveAt(i);
                    i--;
                }

            }

            List<string> load_lst = new List<string>();

            string s = " DL";

            if (add_DeadLoad)
            {

                load_lst.Add("LOAD 1 DEAD LOAD");
                load_lst.Add("MEMBER LOAD");
                load_lst.Add(string.Format("{0} UNI GY -{1:f4}", Deck_Analysis.Inner_Girders_as_String, deck_member_load[0]));

                load_lst.Add("LOAD 2 SIDL EXCEPT SURFACING");
                load_lst.Add("MEMBER LOAD");
                load_lst.Add(string.Format("{0} UNI GY -{1:f4}", Deck_Analysis.Inner_Girders_as_String, deck_member_load[1]));

                load_lst.Add("LOAD 3 SIDL SURFACING");
                load_lst.Add("MEMBER LOAD");
                load_lst.Add(string.Format("{0} UNI GY -{1:f4}", Deck_Analysis.Inner_Girders_as_String, deck_member_load[2]));

                load_lst.Add("LOAD 4 FPLL");
                load_lst.Add("MEMBER LOAD");
                load_lst.Add(string.Format("{0} UNI GY -{1:f4}", Deck_Analysis.Inner_Girders_as_String, 0.0));
                
            }
            else
            {
                load_lst.Add("LOAD 1");
                load_lst.Add("MEMBER LOAD");
                load_lst.Add("1 TO " + Deck_Analysis.MemColls.Count + " UNI GY -0.001");
            }

            //Bridge_Analysis.LoadReadFromGrid(dgv_live_load);

            //Bridge_Analysis.Live_Load_List = iApp.LiveLoads;
            Deck_Analysis.Live_Load_List = LoadData.GetLiveLoads(Path.Combine(Path.GetDirectoryName(file_name), "ll.txt"));
            if (add_LiveLoad)
            {

                List<string> list = new List<string>();
                list.Add(string.Format("DEFINE MOVING LOAD FILE LL.TXT"));
              
                if (load_no == 1)
                {
                    list.AddRange(deck_ll_1.ToArray());
                }
                if (load_no == 2)
                {
                    list.AddRange(deck_ll_2.ToArray());
                }
                if (load_no == 3)
                {
                    list.AddRange(deck_ll_3.ToArray());

                }
                if (load_no == 4)
                {
                    list.AddRange(deck_ll_4.ToArray());

                }
                if (load_no == 5)
                {
                    list.AddRange(deck_ll_5.ToArray());

                }
                if (load_no == 6)
                {
                    list.AddRange(deck_ll_6.ToArray());
                }
                load_lst.AddRange(list.ToArray());
            }
            inp_file_cont.InsertRange(indx, load_lst);
            //inp_file_cont.InsertRange(indx, );
            File.WriteAllLines(file_name, inp_file_cont.ToArray());
            //MessageBox.Show(this, "Load data is added in file " + file_name, "ASTRA", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        void Ana_Show_Moment_Shear_2013_09_23()
        {
            MemberCollection mc = new MemberCollection(Minor_Analysis.TotalLoad_Analysis.Analysis.Members);

            MemberCollection sort_membs = new MemberCollection();

            JointNodeCollection jn_col = Minor_Analysis.TotalLoad_Analysis.Analysis.Joints;

            double L = Minor_Analysis.Length;
            double W = Minor_Analysis.WidthBridge;
            double val = L / 2;
            int i = 0;

            List<int> _support_inn_joints = new List<int>();
            List<int> _deff_inn_joints = new List<int>();
            List<int> _L8_inn_joints = new List<int>();
            List<int> _L4_inn_joints = new List<int>();
            List<int> _3L8_inn_joints = new List<int>();
            List<int> _L2_inn_joints = new List<int>();



            List<int> _support_out_joints = new List<int>();
            List<int> _deff_out_joints = new List<int>();
            List<int> _L8_out_joints = new List<int>();
            List<int> _L4_out_joints = new List<int>();
            List<int> _3L8_out_joints = new List<int>();
            List<int> _L2_out_joints = new List<int>();


            //List<int> _L4_out_joints = new List<int>();
            //List<int> _deff_out_joints = new List<int>();

            List<int> _cross_joints = new List<int>();


            List<double> _X_joints = new List<double>();
            List<double> _Z_joints = new List<double>();

            for (i = 0; i < jn_col.Count; i++)
            {
                if (_X_joints.Contains(jn_col[i].X) == false) _X_joints.Add(jn_col[i].X);
                if (_Z_joints.Contains(jn_col[i].Z) == false) _Z_joints.Add(jn_col[i].Z);
            }
            //val = MyList.StringToDouble(txt_Ana_eff_depth.Text, -999.0);
            val = Deff;


            List<double> _X_min = new List<double>();
            List<double> _X_max = new List<double>();
            double x_max, x_min;
            double vvv = 99999999999999999;
            for (int zc = 0; zc < _Z_joints.Count; zc++)
            {

                x_min = vvv;
                x_max = -vvv;

                for (i = 0; i < jn_col.Count; i++)
                {
                    //if (_X_joints.Contains(jn_col[i].X) == false) _X_joints.Add(jn_col[i].X);
                    //if (_Z_joints.Contains(jn_col[i].Z) == false) _Z_joints.Add(jn_col[i].Z);

                    if (_Z_joints[zc] == jn_col[i].Z)
                    {
                        if (x_min > jn_col[i].X)
                            x_min = jn_col[i].X;
                        if (x_max < jn_col[i].X)
                            x_max = jn_col[i].X;
                    }
                }
                if (x_max != -vvv)
                    _X_max.Add(x_max);
                if (x_min != vvv)
                    _X_min.Add(x_min);
            }

            //val = MyList.StringToDouble(txt_Ana_eff_depth.Text, -999.0);
            val = Minor_Analysis.TotalLoad_Analysis.Analysis.Effective_Depth;
            //if (_X_joints.Contains(val))
            //{
            //    Bridge_Analysis.Effective_Depth = val;
            //}
            //else
            //{
            //    Bridge_Analysis.Effective_Depth = _X_joints.Count > 1 ? _X_joints[2] : 0.0; ;
            //}
            //double eff_dep = ;

            //_L_2_joints.Clear();

            double cant_wi_left = Minor_Analysis.Width_LeftCantilever;
            double cant_wi_right = Minor_Analysis.Width_RightCantilever;
            //Bridge_Analysis.Width_LeftCantilever = cant_wi;
            //Bridge_Analysis.Width_RightCantilever = _Z_joints[_Z_joints.Count - 1] - _Z_joints[_Z_joints.Count - 3];


            #region Find Joints
            for (i = 0; i < jn_col.Count; i++)
            {
                try
                {
                    if ((jn_col[i].Z >= cant_wi_left && jn_col[i].Z <= (W - cant_wi_right)) == false) continue;
                    x_min = _X_min[_Z_joints.IndexOf(jn_col[i].Z)];

                    if ((jn_col[i].X.ToString("0.0") == ((L / 2.0) + x_min).ToString("0.0")))
                    {
                        if (jn_col[i].Z >= cant_wi_left && jn_col[i].Z <= (W - cant_wi_right))
                            _L2_inn_joints.Add(jn_col[i].NodeNo);
                    }


                    if (jn_col[i].X.ToString("0.0") == ((3 * L / 8.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _3L8_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (3 * L / 8.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _3L8_out_joints.Add(jn_col[i].NodeNo);
                    }



                    if (jn_col[i].X.ToString("0.0") == ((L / 8.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _L8_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (L / 8.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _L8_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if (jn_col[i].X.ToString("0.0") == ((L / 4.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _L4_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (L / 4.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _L4_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if ((jn_col[i].X.ToString("0.0") == (Minor_Analysis.Effective_Depth + x_min).ToString("0.0")))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _deff_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if ((jn_col[i].X.ToString("0.0") == (L - Minor_Analysis.Effective_Depth + x_min).ToString("0.0")))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _deff_out_joints.Add(jn_col[i].NodeNo);
                    }



                    if (jn_col[i].X.ToString("0.0") == (0).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _support_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == (L + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _support_out_joints.Add(jn_col[i].NodeNo);
                    }
                }
                catch (Exception ex) { MessageBox.Show(this, ""); }
            }

            #endregion Find Joints


            if (_L2_inn_joints.Count > 2)
            {
                _L2_inn_joints.RemoveAt(0);
                _L2_inn_joints.RemoveAt(_L2_inn_joints.Count - 1);
            }
            if (_3L8_inn_joints.Count > 2)
            {
                _3L8_inn_joints.RemoveAt(0);
                _3L8_inn_joints.RemoveAt(_3L8_inn_joints.Count - 1);
            }
            if (_3L8_out_joints.Count > 2)
            {
                _3L8_out_joints.RemoveAt(0);
                _3L8_out_joints.RemoveAt(_3L8_out_joints.Count - 1);
            }
            if (_L4_inn_joints.Count > 2)
            {
                _L4_inn_joints.RemoveAt(0);
                _L4_inn_joints.RemoveAt(_L4_inn_joints.Count - 1);
            }
            if (_L4_out_joints.Count > 2)
            {
                _L4_out_joints.RemoveAt(0);
                _L4_out_joints.RemoveAt(_L4_out_joints.Count - 1);
            }

            if (_L8_inn_joints.Count > 2)
            {
                _L8_inn_joints.RemoveAt(0);
                _L8_inn_joints.RemoveAt(_L8_inn_joints.Count - 1);
            }
            if (_L8_out_joints.Count > 2)
            {
                _L8_out_joints.RemoveAt(0);
                _L8_out_joints.RemoveAt(_L8_out_joints.Count - 1);
            }


            if (_deff_inn_joints.Count > 2)
            {
                _deff_inn_joints.RemoveAt(0);
                _deff_inn_joints.RemoveAt(_deff_inn_joints.Count - 1);
            }
            if (_deff_out_joints.Count > 2)
            {
                _deff_out_joints.RemoveAt(0);
                _deff_out_joints.RemoveAt(_deff_out_joints.Count - 1);
            }


            if (_support_inn_joints.Count > 2)
            {
                _support_inn_joints.RemoveAt(0);
                _support_inn_joints.RemoveAt(_support_inn_joints.Count - 1);
            }
            if (_support_out_joints.Count > 2)
            {
                _support_out_joints.RemoveAt(0);
                _support_out_joints.RemoveAt(_support_out_joints.Count - 1);
            }




            _L4_inn_joints.AddRange(_L4_out_joints);
            _deff_inn_joints.AddRange(_deff_out_joints);

            _L8_inn_joints.AddRange(_L8_out_joints);
            _3L8_out_joints.AddRange(_3L8_out_joints);
            _support_out_joints.AddRange(_support_out_joints);


            _cross_joints.AddRange(_L2_inn_joints);
            _cross_joints.AddRange(_L4_inn_joints);
            _cross_joints.AddRange(_deff_inn_joints);



            Result.Clear();
            Result.Add("");
            Result.Add("");
            Result.Add("Analysis Result of RCC T-BEAM Bridge");
            Result.Add("");
       

            #region Null All variables
            mc = null;


            jn_col = null;


            _L2_inn_joints = null;
            _L4_inn_joints = null;
            _deff_inn_joints = null;

            _L4_out_joints = null;
            _deff_out_joints = null;
            #endregion
            //Ana_Show_Cross_Girder();

            File.WriteAllLines(Path.Combine(user_path, "ANALYSIS_RESULT.TXT"), Result.ToArray());

            //iApp.RunExe(Path.Combine(user_path, "ANALYSIS_RESULT.TXT"));
        }

        void Ana_Show_Moment_Shear_2013_09_24()
        {
            MemberCollection mc = new MemberCollection(Minor_Analysis.TotalLoad_Analysis.Analysis.Members);

            MemberCollection sort_membs = new MemberCollection();

            JointNodeCollection jn_col = Minor_Analysis.TotalLoad_Analysis.Analysis.Joints;

            double L = Minor_Analysis.Length;
            double W = Minor_Analysis.WidthBridge;
            double val = L / 2;
            int i = 0;

            List<int> _support_inn_joints = new List<int>();
            List<int> _deff_inn_joints = new List<int>();
            List<int> _L8_inn_joints = new List<int>();
            List<int> _L4_inn_joints = new List<int>();
            List<int> _3L8_inn_joints = new List<int>();
            List<int> _L2_inn_joints = new List<int>();



            List<int> _support_out_joints = new List<int>();
            List<int> _deff_out_joints = new List<int>();
            List<int> _L8_out_joints = new List<int>();
            List<int> _L4_out_joints = new List<int>();
            List<int> _3L8_out_joints = new List<int>();
            List<int> _L2_out_joints = new List<int>();


            //List<int> _L4_out_joints = new List<int>();
            //List<int> _deff_out_joints = new List<int>();

            List<int> _cross_joints = new List<int>();


            List<double> _X_joints = new List<double>();
            List<double> _Z_joints = new List<double>();

            for (i = 0; i < jn_col.Count; i++)
            {
                if (_X_joints.Contains(jn_col[i].X) == false) _X_joints.Add(jn_col[i].X);
                if (_Z_joints.Contains(jn_col[i].Z) == false) _Z_joints.Add(jn_col[i].Z);
            }
            //val = MyList.StringToDouble(txt_Ana_eff_depth.Text, -999.0);
            val = Deff;


            List<double> _X_min = new List<double>();
            List<double> _X_max = new List<double>();
            double x_max, x_min;
            double vvv = 99999999999999999;
            for (int zc = 0; zc < _Z_joints.Count; zc++)
            {

                x_min = vvv;
                x_max = -vvv;

                for (i = 0; i < jn_col.Count; i++)
                {
                    //if (_X_joints.Contains(jn_col[i].X) == false) _X_joints.Add(jn_col[i].X);
                    //if (_Z_joints.Contains(jn_col[i].Z) == false) _Z_joints.Add(jn_col[i].Z);

                    if (_Z_joints[zc] == jn_col[i].Z)
                    {
                        if (x_min > jn_col[i].X)
                            x_min = jn_col[i].X;
                        if (x_max < jn_col[i].X)
                            x_max = jn_col[i].X;
                    }
                }
                if (x_max != -vvv)
                    _X_max.Add(x_max);
                if (x_min != vvv)
                    _X_min.Add(x_min);
            }

            //val = MyList.StringToDouble(txt_Ana_eff_depth.Text, -999.0);
            val = Minor_Analysis.TotalLoad_Analysis.Analysis.Effective_Depth;
            //if (_X_joints.Contains(val))
            //{
            //    Bridge_Analysis.Effective_Depth = val;
            //}
            //else
            //{
            //    Bridge_Analysis.Effective_Depth = _X_joints.Count > 1 ? _X_joints[2] : 0.0; ;
            //}
            //double eff_dep = ;

            //_L_2_joints.Clear();

            double cant_wi_left = Minor_Analysis.Width_LeftCantilever;
            double cant_wi_right = Minor_Analysis.Width_RightCantilever;
            //Bridge_Analysis.Width_LeftCantilever = cant_wi;
            //Bridge_Analysis.Width_RightCantilever = _Z_joints[_Z_joints.Count - 1] - _Z_joints[_Z_joints.Count - 3];


            #region Find Joints
            for (i = 0; i < jn_col.Count; i++)
            {
                try
                {
                    if ((jn_col[i].Z >= cant_wi_left && jn_col[i].Z <= (W - cant_wi_right)) == false) continue;
                    x_min = _X_min[_Z_joints.IndexOf(jn_col[i].Z)];

                    if ((jn_col[i].X.ToString("0.0") == ((L / 2.0) + x_min).ToString("0.0")))
                    {
                        if (jn_col[i].Z >= cant_wi_left && jn_col[i].Z <= (W - cant_wi_right))
                            _L2_inn_joints.Add(jn_col[i].NodeNo);
                    }

                    if (jn_col[i].X.ToString("0.0") == ((3 * L / 8.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _3L8_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (3 * L / 8.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _3L8_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if (jn_col[i].X.ToString("0.0") == ((L / 8.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _L8_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (L / 8.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _L8_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if (jn_col[i].X.ToString("0.0") == ((L / 4.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _L4_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (L / 4.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _L4_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if ((jn_col[i].X.ToString("0.0") == (Minor_Analysis.Effective_Depth + x_min).ToString("0.0")))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _deff_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if ((jn_col[i].X.ToString("0.0") == (L - Minor_Analysis.Effective_Depth + x_min).ToString("0.0")))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _deff_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if (jn_col[i].X.ToString("0.0") == (x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _support_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == (L + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _support_out_joints.Add(jn_col[i].NodeNo);
                    }
                }
                catch (Exception ex) { MessageBox.Show(this, ""); }
            }

            #endregion Find Joints

            Result.Clear();
            Result.Add("");
            Result.Add("");
            Result.Add("Analysis Result of RCC T-BEAM Bridge");
            Result.Add("");

            #region SHEAR FORCE
            List<double> SF_DL_Self_Weight_G1 = new List<double>();
            List<double> SF_DL_Self_Weight_G2 = new List<double>();

            List<double> SF__DL_Deck_Wet_Conc_G1 = new List<double>();
            List<double> SF__DL_Deck_Wet_Conc_G2 = new List<double>();
            
            List<double> SF_DL_Deck_Dry_Conc_G1 = new List<double>();
            List<double> SF_DL_Deck_Dry_Conc_G2 = new List<double>();
            
            List<double> SF_DL_Self_Deck_G1 = new List<double>();
            List<double> SF_DL_Self_Deck_G2 = new List<double>();
            List<double> SF_DL_Self_Deck_G3 = new List<double>();
            List<double> SF_DL_Self_Deck_G4 = new List<double>();
            
            List<double> SF_SIDL_Crash_Barrier_G1 = new List<double>();
            List<double> SF_SIDL_Crash_Barrier_G2 = new List<double>();
            List<double> SF_SIDL_Crash_Barrier_G3 = new List<double>();
            List<double> SF_SIDL_Crash_Barrier_G4 = new List<double>();

            List<double> SF_SIDL_Wearing_G1 = new List<double>();
            List<double> SF_SIDL_Wearing_G2 = new List<double>();
            List<double> SF_SIDL_Wearing_G3 = new List<double>();
            List<double> SF_SIDL_Wearing_G4 = new List<double>();
            #endregion SHEAR FORCE

            #region Summary of B.M & SF due to Dead Load ( Forces due to Self weight of girder) Knm

            #endregion Summary of B.M & SF due to Dead Load ( Forces due to Self weight of girder) Knm


            MaxForce f = null;

            for (i = 1; i < 6; i++)
            {
                #region Dead Load 1
                if (i == 1)
                {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(val);
                    SF_DL_Self_Weight_G2.Add(val);


                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(val);
                    SF_DL_Self_Weight_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(val);
                    SF_DL_Self_Weight_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(val);
                    SF_DL_Self_Weight_G2.Add(val);



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(val);
                    SF_DL_Self_Weight_G2.Add(val);

                }
                #endregion Dead Load 1

                #region Dead Load 2
                else if (i == 2)
                {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF__DL_Deck_Wet_Conc_G1.Add(val);
                    SF__DL_Deck_Wet_Conc_G2.Add(val);


                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF__DL_Deck_Wet_Conc_G1.Add(val);
                    SF__DL_Deck_Wet_Conc_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF__DL_Deck_Wet_Conc_G1.Add(val);
                    SF__DL_Deck_Wet_Conc_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF__DL_Deck_Wet_Conc_G1.Add(val);
                    SF__DL_Deck_Wet_Conc_G2.Add(val);



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF__DL_Deck_Wet_Conc_G1.Add(val);
                    SF__DL_Deck_Wet_Conc_G2.Add(val);

                }
                #endregion Dead Load 1

                #region Dead Load 3
                else if (i == 3)
                {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(val);
                    SF_DL_Deck_Dry_Conc_G2.Add(val);


                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(val);
                    SF_DL_Deck_Dry_Conc_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(val);
                    SF_DL_Deck_Dry_Conc_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(val);
                    SF_DL_Deck_Dry_Conc_G2.Add(val);



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(val);
                    SF_DL_Deck_Dry_Conc_G2.Add(val);

                }
                #endregion Dead Load 3


                #region Dead Load 4
                else if (i == 4)
                {
                    #region Support
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[2], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[3], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G4.Add(val);

                    #endregion Support

                    #region L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[2], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[3], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G4.Add(val);

                    #endregion  L / 8


                    #region L / 4
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[2], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[3], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G4.Add(val);

                    #endregion  L / 8

                    #region 3L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[2], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[3], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G4.Add(val);

                    #endregion  L / 8

                    #region L / 2
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[2], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[3], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G4.Add(val);

                    #endregion  L / 8


                }
                #endregion Dead Load 4


                #region Dead Load 5
                else if (i == 5)
                {
                    #region Support
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G4.Add(val);

                    #endregion Support

                    #region L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G4.Add(val);

                    #endregion  L / 8

                    #region L / 4
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G4.Add(val);

                    #endregion  L / 8

                    #region 3L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G4.Add(val);

                    #endregion  L / 8

                    #region L / 2
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G4.Add(val);

                    #endregion  L / 2

                }
                #endregion Dead Load 5


                #region Dead Load 5
                else if (i == 5)
                {
                    #region Support
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G4.Add(val);

                    #endregion Support

                    #region L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G4.Add(val);

                    #endregion  L / 8

                    #region L / 4
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G4.Add(val);

                    #endregion  L / 8

                    #region 3L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G4.Add(val);

                    #endregion  L / 8

                    #region L / 2
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G4.Add(val);

                    #endregion  L / 2

                }
                #endregion Dead Load 5


                val = 0.0;

            }

            val = 0.0;



             

            #region Null All variables
            mc = null;


            jn_col = null;


            _L2_inn_joints = null;
            _L4_inn_joints = null;
            _deff_inn_joints = null;

            _L4_out_joints = null;
            _deff_out_joints = null;
            #endregion
            //Ana_Show_Cross_Girder();

            File.WriteAllLines(Path.Combine(user_path, "ANALYSIS_RESULT.TXT"), Result.ToArray());

            //iApp.RunExe(Path.Combine(user_path, "ANALYSIS_RESULT.TXT"));
        }

        void Show_Long_Girder_Moment_Shear()
        {
            Show_Bearing_Forces();



            MemberCollection mc = new MemberCollection(Minor_Analysis.TotalLoad_Analysis.Analysis.Members);

            MemberCollection sort_membs = new MemberCollection();

            JointNodeCollection jn_col = Minor_Analysis.TotalLoad_Analysis.Analysis.Joints;




            double L = Minor_Analysis.Length;
            double W = Minor_Analysis.WidthBridge;
            double val = L / 2;
            int i = 0;

            List<int> _support_inn_joints = new List<int>();
            List<int> _deff_inn_joints = new List<int>();
            List<int> _L8_inn_joints = new List<int>();
            List<int> _L4_inn_joints = new List<int>();
            List<int> _3L8_inn_joints = new List<int>();
            List<int> _L2_inn_joints = new List<int>();



            List<int> _3L16_inn_joints = new List<int>();
            List<int> _5L16_inn_joints = new List<int>();
            List<int> _7L16_inn_joints = new List<int>();



            List<int> _support_out_joints = new List<int>();
            List<int> _deff_out_joints = new List<int>();
            List<int> _L8_out_joints = new List<int>();
            List<int> _L4_out_joints = new List<int>();
            List<int> _3L8_out_joints = new List<int>();
            List<int> _L2_out_joints = new List<int>();


            List<int> _3L16_out_joints = new List<int>();
            List<int> _5L16_out_joints = new List<int>();
            List<int> _7L16_out_joints = new List<int>();



            //List<int> _L4_out_joints = new List<int>();
            //List<int> _deff_out_joints = new List<int>();

            List<int> _cross_joints = new List<int>();


            List<double> _X_joints = new List<double>();
            List<double> _Z_joints = new List<double>();

            for (i = 0; i < jn_col.Count; i++)
            {
                if (_X_joints.Contains(jn_col[i].X) == false) _X_joints.Add(jn_col[i].X);
                if (_Z_joints.Contains(jn_col[i].Z) == false) _Z_joints.Add(jn_col[i].Z);
            }
            //val = MyList.StringToDouble(txt_Ana_eff_depth.Text, -999.0);
            val = Deff;

            List<double> _X_min = new List<double>();
            List<double> _X_max = new List<double>();
            double x_max, x_min;
            double vvv = 99999999999999999;
            for (int zc = 0; zc < _Z_joints.Count; zc++)
            {
                x_min = vvv;
                x_max = -vvv;

                for (i = 0; i < jn_col.Count; i++)
                {
                    //if (_X_joints.Contains(jn_col[i].X) == false) _X_joints.Add(jn_col[i].X);
                    //if (_Z_joints.Contains(jn_col[i].Z) == false) _Z_joints.Add(jn_col[i].Z);

                    if (_Z_joints[zc] == jn_col[i].Z)
                    {
                        if (x_min > jn_col[i].X)
                            x_min = jn_col[i].X;
                        if (x_max < jn_col[i].X)
                            x_max = jn_col[i].X;
                    }
                }
                if (x_max != -vvv)
                    _X_max.Add(x_max);
                if (x_min != vvv)
                    _X_min.Add(x_min);
            }

            //val = MyList.StringToDouble(txt_Ana_eff_depth.Text, -999.0);
            val = Minor_Analysis.TotalLoad_Analysis.Analysis.Effective_Depth;
            //if (_X_joints.Contains(val))
            //{
            //    Bridge_Analysis.Effective_Depth = val;
            //}
            //else
            //{
            //    Bridge_Analysis.Effective_Depth = _X_joints.Count > 1 ? _X_joints[2] : 0.0; ;
            //}
            //double eff_dep = ;

            //_L_2_joints.Clear();

            double cant_wi_left = Minor_Analysis.Width_LeftCantilever;
            double cant_wi_right = Minor_Analysis.Width_RightCantilever;
            //Bridge_Analysis.Width_LeftCantilever = cant_wi;
            //Bridge_Analysis.Width_RightCantilever = _Z_joints[_Z_joints.Count - 1] - _Z_joints[_Z_joints.Count - 3];


            #region Find Joints
            for (i = 0; i < jn_col.Count; i++)
            {
                try
                {
                    if ((jn_col[i].Z >= cant_wi_left && jn_col[i].Z <= (W - cant_wi_right)) == false) continue;
                    x_min = _X_min[_Z_joints.IndexOf(jn_col[i].Z)];

                    if ((jn_col[i].X.ToString("0.0") == ((L / 2.0) + x_min).ToString("0.0")))
                    {
                        if (jn_col[i].Z >= cant_wi_left && jn_col[i].Z <= (W - cant_wi_right))
                            _L2_inn_joints.Add(jn_col[i].NodeNo);
                    }

                    if (jn_col[i].X.ToString("0.0") == ((3 * L / 8.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _3L8_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (3 * L / 8.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _3L8_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if (jn_col[i].X.ToString("0.0") == ((L / 8.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _L8_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (L / 8.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _L8_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if (jn_col[i].X.ToString("0.0") == ((L / 4.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _L4_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (L / 4.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _L4_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if ((jn_col[i].X.ToString("0.0") == (Minor_Analysis.Effective_Depth + x_min).ToString("0.0")))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _deff_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if ((jn_col[i].X.ToString("0.0") == (L - Minor_Analysis.Effective_Depth + x_min).ToString("0.0")))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _deff_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if (jn_col[i].X.ToString("0.0") == (x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _support_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == (L + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _support_out_joints.Add(jn_col[i].NodeNo);
                    }

                    #region 3L/16


                    if (jn_col[i].X.ToString("0.0") == ((3 * L / 16.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _3L16_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (3 * L / 16.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _3L16_out_joints.Add(jn_col[i].NodeNo);
                    }


                    #endregion 3L/16



                    #region 5L/16


                    if (jn_col[i].X.ToString("0.0") == ((5 * L / 16.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _5L16_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (5 * L / 16.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _5L16_out_joints.Add(jn_col[i].NodeNo);
                    }


                    #endregion 3L/16



                    #region 7L/16


                    if (jn_col[i].X.ToString("0.0") == ((7 * L / 16.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _7L16_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (7 * L / 16.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _7L16_out_joints.Add(jn_col[i].NodeNo);
                    }


                    #endregion 3L/16



                }
                catch (Exception ex) { MessageBox.Show(this, ""); }
            }

            #endregion Find Joints

            Result.Clear();
            Result.Add("");
            Result.Add("");
            //Result.Add("Analysis Result of RCC T-BEAM Bridge");
            Result.Add("");

            #region SHEAR FORCE
            List<double> SF_DL_Self_Weight_G1 = new List<double>();
            List<double> SF_DL_Self_Weight_G2 = new List<double>();

            List<double> SF_DL_Deck_Wet_Conc_G1 = new List<double>();
            List<double> SF_DL_Deck_Wet_Conc_G2 = new List<double>();

            List<double> SF_DL_Deck_Dry_Conc_G1 = new List<double>();
            List<double> SF_DL_Deck_Dry_Conc_G2 = new List<double>();

            List<double> SF_DL_Self_Deck_G1 = new List<double>();
            List<double> SF_DL_Self_Deck_G2 = new List<double>();
            List<double> SF_DL_Self_Deck_G3 = new List<double>();
            List<double> SF_DL_Self_Deck_G4 = new List<double>();

            List<double> SF_SIDL_Crash_Barrier_G1 = new List<double>();
            List<double> SF_SIDL_Crash_Barrier_G2 = new List<double>();
            List<double> SF_SIDL_Crash_Barrier_G3 = new List<double>();
            List<double> SF_SIDL_Crash_Barrier_G4 = new List<double>();

            List<double> SF_SIDL_Wearing_G1 = new List<double>();
            List<double> SF_SIDL_Wearing_G2 = new List<double>();
            List<double> SF_SIDL_Wearing_G3 = new List<double>();
            List<double> SF_SIDL_Wearing_G4 = new List<double>();


            List<double> SF_LL_1 = new List<double>();
            List<double> SF_LL_2 = new List<double>();
            List<double> SF_LL_3 = new List<double>();
            List<double> SF_LL_4 = new List<double>();
            List<double> SF_LL_5 = new List<double>();
            List<double> SF_LL_6 = new List<double>();

            List<List<double>> SF_LL_List = new List<List<double>>();


             
            MaxForce f = null;



            for (i = 1; i <= 6; i++)
            {
                #region Dead Load 1
                if (i == 1)
                {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Self_Weight_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Self_Weight_G2.Add(Math.Abs(val));


                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Self_Weight_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Self_Weight_G2.Add(Math.Abs(val));



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G2.Add(Math.Abs(val));

                }
                #endregion Dead Load 1

                #region Dead Load 2
                else if (i == 2)
                {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));


                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));


                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));

                }
                #endregion Dead Load 1

                #region Dead Load 3
                else if (i == 3)
                {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));


                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));

                }
                #endregion Dead Load 3

                #region Dead Load 4
                else if (i == 4)
                {
                    #region Support
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(Math.Abs(val));

                    if (_support_inn_joints.Count > 2)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_DL_Self_Deck_G3.Add(Math.Abs(val));
                    if (_support_inn_joints.Count > 3)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;
                    SF_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion Support

                    #region Deff
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(Math.Abs(val));

                    if (_deff_inn_joints.Count > 2)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_DL_Self_Deck_G3.Add(Math.Abs(val));

                    if (_deff_inn_joints.Count > 3)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion Support


                    //if (.Count > 3)
                    //{

                    //}
                    //else
                    //    val = 0.0;


                    #region L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(Math.Abs(val));



                    if (_L8_inn_joints.Count > 2)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_DL_Self_Deck_G3.Add(Math.Abs(val));

                    if (_L8_inn_joints.Count > 3)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion  L / 8


                    #region L / 4
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(Math.Abs(val));

                    if (_L4_inn_joints.Count > 2)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_DL_Self_Deck_G3.Add(Math.Abs(val));
                    if (_L4_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[3], i);
                    val = f.Force;

                    }
                    else
                        val = 0.0;

                    SF_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region 3L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(Math.Abs(val));

                    if (_3L8_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[2], i);
                    val = f.Force;

                    }
                    else
                        val = 0.0;

                    SF_DL_Self_Deck_G3.Add(Math.Abs(val));
                    if (_3L8_inn_joints.Count > 3)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 2
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(Math.Abs(val));

                    if (_L2_inn_joints.Count > 2)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_DL_Self_Deck_G3.Add(Math.Abs(val));
                    if (_L2_inn_joints.Count > 3)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion  L / 2


                }
                #endregion Dead Load 4

                #region Dead Load 5
                else if (i == 5)
                {
                    #region Support
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));


                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));
                    if (_support_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[2], i);
                    val = f.Force;

                    }
                    else
                        val = 0.0;
                    SF_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));
                    if (_support_inn_joints.Count > 3)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion Support


                    #region Deff
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));
                    if (_deff_inn_joints.Count > 2)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));
                    if (_deff_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion Deff


                    #region L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));
                    if (_L8_inn_joints.Count > 2)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));

                    if (_L8_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 4
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));
                    
                    if (_L4_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[2], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));
                    
                    if (_L4_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[3], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region 3L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));
                    
                    if (_3L8_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[2], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));
                    
                    if (_3L8_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[3], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 2
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));
                    
                    if (_L2_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[2], i);
                    val = f.Force;

                    }
                    else
                        val = 0.0;

                    SF_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));
                    
                    if (_L2_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[3], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion  L / 2

                }
                #endregion Dead Load 5

                #region Dead Load 6
                else if (i == 6)
                {
                    #region Support
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(Math.Abs(val));
                    
                    if (_support_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[2], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Wearing_G3.Add(Math.Abs(val));
                    
                    if (_support_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[3], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion Support

                    #region Deff
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(Math.Abs(val));
                    
                    if (_deff_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[2], i);
                    val = f.Force;

                    }
                    else
                        val = 0.0;
                    SF_SIDL_Wearing_G3.Add(Math.Abs(val));
                    
                    if (_deff_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[3], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion Deff

                    #region L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(Math.Abs(val));
                    
                    if (_L8_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[2], i);
                    val = f.Force;

                    }
                    else
                        val = 0.0;
                    SF_SIDL_Wearing_G3.Add(Math.Abs(val));
                    
                    if (_L8_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[3], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 4
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(Math.Abs(val));
                    
                    if (_L4_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[2], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Wearing_G3.Add(Math.Abs(val));
                    
                    if (_L4_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[3], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region 3L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(Math.Abs(val));
                    
                    if (_3L8_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[2], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Wearing_G3.Add(Math.Abs(val));
                    
                    if (_3L8_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[3], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 2
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(Math.Abs(val));
                    
                    if (_L2_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[2], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Wearing_G3.Add(Math.Abs(val));
                    
                    if (_L2_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[3], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion  L / 2

                }
                #endregion Dead Load 5
            }

            List<int> tmp_jts = new List<int>();

            #region Live Load SHEAR Force




            #region Live Load 1
            for (i = 0; i < all_loads.Count; i++)
            {

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.All_LL_Analysis[i].GetJoint_ShearForce(_support_inn_joints, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.All_LL_Analysis[i].GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.All_LL_Analysis[i].GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.All_LL_Analysis[i].GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.All_LL_Analysis[i].GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.All_LL_Analysis[i].GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;

                SF_LL_1.Add(Math.Abs(val));

                MyList.Array_Multiply_With(ref SF_LL_1, 10.0);

                SF_LL_List.Add(SF_LL_1);

                SF_LL_1 = new List<double>();

            }
            #endregion Live Load Analysis

            if (false)
            {

                #region Live Load 1

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_ShearForce(_support_inn_joints, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));

                #endregion Support

                #region Live Load 2

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_ShearForce(_support_inn_joints, true);
                val = f.Force;
                SF_LL_2.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_2.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_2.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_2.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_2.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_2.Add(Math.Abs(val));

                #endregion Load 2

                #region Live Load 3

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_ShearForce(_support_inn_joints, true);
                val = f.Force;
                SF_LL_3.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_3.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_3.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_3.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_3.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_3.Add(Math.Abs(val));

                #endregion Load 2

                #region Live Load 4

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_ShearForce(_support_inn_joints, true);
                val = f.Force;
                SF_LL_4.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_4.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_4.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_4.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_4.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_4.Add(Math.Abs(val));

                #endregion Support

                #region Live Load 5

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_ShearForce(_support_inn_joints, true);
                val = f.Force;
                SF_LL_5.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_5.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_5.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_5.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_5.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_5.Add(Math.Abs(val));

                #endregion Support

                #region Live Load 6

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_ShearForce(_support_inn_joints, true);
                val = f.Force;
                SF_LL_6.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_6.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_6.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_6.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_6.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_6.Add(Math.Abs(val));

                #endregion Support



                MyList.Array_Multiply_With(ref SF_LL_1, 10.0);
                MyList.Array_Multiply_With(ref SF_LL_2, 10.0);
                MyList.Array_Multiply_With(ref SF_LL_3, 10.0);
                MyList.Array_Multiply_With(ref SF_LL_4, 10.0);
                MyList.Array_Multiply_With(ref SF_LL_5, 10.0);
                MyList.Array_Multiply_With(ref SF_LL_6, 10.0);
            }

            #endregion Live Load




            MyList.Array_Multiply_With(ref SF_DL_Self_Weight_G1, 10.0);
            MyList.Array_Multiply_With(ref SF_DL_Self_Weight_G2, 10.0);

            MyList.Array_Multiply_With(ref SF_DL_Deck_Wet_Conc_G1, 10.0);
            MyList.Array_Multiply_With(ref SF_DL_Deck_Wet_Conc_G2, 10.0);

            MyList.Array_Multiply_With(ref SF_DL_Deck_Dry_Conc_G1, 10.0);
            MyList.Array_Multiply_With(ref SF_DL_Deck_Dry_Conc_G2, 10.0);

            MyList.Array_Multiply_With(ref SF_DL_Self_Deck_G1, 10.0);
            MyList.Array_Multiply_With(ref SF_DL_Self_Deck_G2, 10.0);
            MyList.Array_Multiply_With(ref SF_DL_Self_Deck_G3, 10.0);
            MyList.Array_Multiply_With(ref SF_DL_Self_Deck_G4, 10.0);

            MyList.Array_Multiply_With(ref SF_SIDL_Crash_Barrier_G1, 10.0);
            MyList.Array_Multiply_With(ref SF_SIDL_Crash_Barrier_G2, 10.0);
            MyList.Array_Multiply_With(ref SF_SIDL_Crash_Barrier_G3, 10.0);
            MyList.Array_Multiply_With(ref SF_SIDL_Crash_Barrier_G4, 10.0);

            MyList.Array_Multiply_With(ref SF_SIDL_Wearing_G1, 10.0);
            MyList.Array_Multiply_With(ref SF_SIDL_Wearing_G2, 10.0);
            MyList.Array_Multiply_With(ref SF_SIDL_Wearing_G3, 10.0);
            MyList.Array_Multiply_With(ref SF_SIDL_Wearing_G4, 10.0);




            #endregion SHEAR FORCE

            #region Bending Moment
            List<double> BM_DL_Self_Weight_G1 = new List<double>();
            List<double> BM_DL_Self_Weight_G2 = new List<double>();

            List<double> BM_DL_Deck_Wet_Conc_G1 = new List<double>();
            List<double> BM_DL_Deck_Wet_Conc_G2 = new List<double>();

            List<double> BM_DL_Deck_Dry_Conc_G1 = new List<double>();
            List<double> BM_DL_Deck_Dry_Conc_G2 = new List<double>();

            List<double> BM_DL_Self_Deck_G1 = new List<double>();
            List<double> BM_DL_Self_Deck_G2 = new List<double>();
            List<double> BM_DL_Self_Deck_G3 = new List<double>();
            List<double> BM_DL_Self_Deck_G4 = new List<double>();

            List<double> BM_SIDL_Crash_Barrier_G1 = new List<double>();
            List<double> BM_SIDL_Crash_Barrier_G2 = new List<double>();
            List<double> BM_SIDL_Crash_Barrier_G3 = new List<double>();
            List<double> BM_SIDL_Crash_Barrier_G4 = new List<double>();

            List<double> BM_SIDL_Wearing_G1 = new List<double>();
            List<double> BM_SIDL_Wearing_G2 = new List<double>();
            List<double> BM_SIDL_Wearing_G3 = new List<double>();
            List<double> BM_SIDL_Wearing_G4 = new List<double>();



            List<double> BM_LL_1 = new List<double>();
            List<double> BM_LL_2 = new List<double>();
            List<double> BM_LL_3 = new List<double>();
            List<double> BM_LL_4 = new List<double>();
            List<double> BM_LL_5 = new List<double>();
            List<double> BM_LL_6 = new List<double>();


            List<List<double>> BM_LL_List = new List<List<double>>();


             

            for (i = 1; i <= 6; i++)
            {
                #region Dead Load 1
                if (i == 1)
                {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G2.Add(Math.Abs(val));



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G2.Add(Math.Abs(val));



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[1], i);
                    val = f.Force;
                    BM_DL_Self_Weight_G2.Add(Math.Abs(val));

                }
                #endregion Dead Load 1

                #region Dead Load 2
                else if (i == 2)
                {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[1], i);
                    val = f.Force;
                    BM_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));


                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[1], i);
                    val = f.Force;
                    BM_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[1], i);
                    val = f.Force;
                    BM_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[1], i);
                    val = f.Force;
                    BM_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));

                }
                #endregion Dead Load 1

                #region Dead Load 3
                else if (i == 3)
                {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[1], i);
                    val = f.Force;
                    BM_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[1], i);
                    val = f.Force;
                    BM_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[1], i);
                    val = f.Force;
                    BM_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[1], i);
                    val = f.Force;
                    BM_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[1], i);
                    val = f.Force;
                    BM_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));

                }
                #endregion Dead Load 3

                #region Dead Load 4
                else if (i == 4)
                {
                    #region Support
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G2.Add(Math.Abs(val));
                    
                    if (_support_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[2], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;

                    BM_DL_Self_Deck_G3.Add(Math.Abs(val));
                    
                    if (_support_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[3], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;

                    BM_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion Support

                    #region Deff
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G2.Add(Math.Abs(val));
                    
                    if (_deff_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[2], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_DL_Self_Deck_G3.Add(Math.Abs(val));
                    
                    if (_deff_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[3], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion Deff

                    #region L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G2.Add(Math.Abs(val));
                    
                    if (_L8_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[2], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_DL_Self_Deck_G3.Add(Math.Abs(val));
                    
                    if (_L8_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[3], i);
                    val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 4
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G2.Add(Math.Abs(val));
                    
                    if (_L4_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[2], i);
                    val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_DL_Self_Deck_G3.Add(Math.Abs(val));
                    
                    if (_L4_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[3], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region 3L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G2.Add(Math.Abs(val));
                    
                    if (_3L8_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[2], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_DL_Self_Deck_G3.Add(Math.Abs(val));
                    
                    if (_3L8_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[3], i);
                    val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 2
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G2.Add(Math.Abs(val));
                    
                    if (_L2_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[2], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_DL_Self_Deck_G3.Add(Math.Abs(val));
                    
                    if (_L2_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[3], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion  L / 8
                }
                #endregion Dead Load 4

                #region Dead Load 5
                else if (i == 5)
                {
                    #region Support
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));
                    
                    if (_support_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[2], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));
                    
                    if (_support_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[3], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion Support



                    #region Deff
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));
                    
                    if (_deff_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[2], i);
                    val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));
                    
                    if (_deff_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[3], i);
                    val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion Deff


                    #region L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));
                    
                    if (_L8_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[2], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));
                    
                    if (_L8_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[3], i);
                    val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 4
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));
                    
                    if (_L4_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[2], i);
                    val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));
                    
                    if (_L4_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[3], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region 3L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));
                    
                    if (_3L8_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[2], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));
                    
                    if (_3L8_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[3], i);
                    val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 2
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));
                    
                    if (_L2_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[2], i);
                    val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));
                    
                    if (_L2_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[3], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion  L / 2

                }
                #endregion Dead Load 5

                #region Dead Load 6
                else if (i == 6)
                {
                    #region Support
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G2.Add(Math.Abs(val));
                    
                    if (_support_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[2], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Wearing_G3.Add(Math.Abs(val));
                    
                    if (_support_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[3], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion Support



                    #region Deff
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G2.Add(Math.Abs(val));
                    
                    if (_deff_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[2], i);
                    val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_SIDL_Wearing_G3.Add(Math.Abs(val));
                    
                    if (_deff_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[3], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion Deff


                    #region L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G2.Add(Math.Abs(val));
                    
                    if (_L8_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[2], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Wearing_G3.Add(Math.Abs(val));
                    
                    if (_L8_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[3], i);
                    val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 4
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G2.Add(Math.Abs(val));
                    
                    if (_L4_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[2], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Wearing_G3.Add(Math.Abs(val));
                    
                    if (_L4_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[3], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;

                    BM_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region 3L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G2.Add(Math.Abs(val));
                    
                    if (_3L8_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[2], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;

                    BM_SIDL_Wearing_G3.Add(Math.Abs(val));
                    
                    if (_3L8_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[3], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;

                    BM_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 2
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G2.Add(Math.Abs(val));
                    
                    if (_L2_inn_joints.Count > 2)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[2], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;

                    BM_SIDL_Wearing_G3.Add(Math.Abs(val));
                    
                    if (_L2_inn_joints.Count > 3)
                    {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[3], i);
                    val = f.Force;
                    }
                    else
                        val = 0.0;

                    BM_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion  L / 2

                }
                #endregion Dead Load 5
            }


            #region Live Load Bending Moments

            for (i = 0; i < all_loads.Count; i++)
            {
                #region Live Load 1

                BM_LL_1.Clear();

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.All_LL_Analysis[i].GetJoint_MomentForce(_support_inn_joints, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.All_LL_Analysis[i].GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.All_LL_Analysis[i].GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.All_LL_Analysis[i].GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.All_LL_Analysis[i].GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.All_LL_Analysis[i].GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));

                #endregion Support


                BM_LL_List.Add(BM_LL_1);
                BM_LL_1 = new List<double>();
            }

            if (false)
            {
                #region Live Load 1

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_MomentForce(_support_inn_joints, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));

                #endregion Support

                #region Live Load 2

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_MomentForce(_support_inn_joints, true);
                val = f.Force;
                BM_LL_2.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_2.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_2.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_2.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_2.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_2.Add(Math.Abs(val));

                #endregion Load 2

                #region Live Load 3

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_MomentForce(_support_inn_joints, true);
                val = f.Force;
                BM_LL_3.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_3.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_3.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_3.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_3.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_3.Add(Math.Abs(val));

                #endregion Load 2

                #region Live Load 4

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_MomentForce(_support_inn_joints, true);
                val = f.Force;
                BM_LL_4.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_4.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_4.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_4.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_4.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_4.Add(Math.Abs(val));

                #endregion Support

                #region Live Load 1

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_MomentForce(_support_inn_joints, true);
                val = f.Force;
                BM_LL_5.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_5.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_5.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_5.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_5.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_5.Add(Math.Abs(val));

                #endregion Support

                #region Live Load 1

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_MomentForce(_support_inn_joints, true);
                val = f.Force;
                BM_LL_6.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_6.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_6.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_6.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_6.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_6.Add(Math.Abs(val));

                #endregion Support


                MyList.Array_Multiply_With(ref BM_LL_1, 10.0);
                MyList.Array_Multiply_With(ref BM_LL_2, 10.0);
                MyList.Array_Multiply_With(ref BM_LL_3, 10.0);
                MyList.Array_Multiply_With(ref BM_LL_4, 10.0);
                MyList.Array_Multiply_With(ref BM_LL_5, 10.0);
                MyList.Array_Multiply_With(ref BM_LL_6, 10.0);

            }

            #region Deck Slab Data
            //List<double> 
            #endregion Deck Slab Data


            #endregion Live Load SHEAR Force




            MyList.Array_Multiply_With(ref BM_DL_Self_Weight_G1, 10.0);
            MyList.Array_Multiply_With(ref BM_DL_Self_Weight_G2, 10.0);

            MyList.Array_Multiply_With(ref BM_DL_Deck_Wet_Conc_G1, 10.0);
            MyList.Array_Multiply_With(ref BM_DL_Deck_Wet_Conc_G2, 10.0);

            MyList.Array_Multiply_With(ref BM_DL_Deck_Dry_Conc_G1, 10.0);
            MyList.Array_Multiply_With(ref BM_DL_Deck_Dry_Conc_G2, 10.0);

            MyList.Array_Multiply_With(ref BM_DL_Self_Deck_G1, 10.0);
            MyList.Array_Multiply_With(ref BM_DL_Self_Deck_G2, 10.0);
            MyList.Array_Multiply_With(ref BM_DL_Self_Deck_G3, 10.0);
            MyList.Array_Multiply_With(ref BM_DL_Self_Deck_G4, 10.0);

            MyList.Array_Multiply_With(ref BM_SIDL_Crash_Barrier_G1, 10.0);
            MyList.Array_Multiply_With(ref BM_SIDL_Crash_Barrier_G2, 10.0);
            MyList.Array_Multiply_With(ref BM_SIDL_Crash_Barrier_G3, 10.0);
            MyList.Array_Multiply_With(ref BM_SIDL_Crash_Barrier_G4, 10.0);

            MyList.Array_Multiply_With(ref BM_SIDL_Wearing_G1, 10.0);
            MyList.Array_Multiply_With(ref BM_SIDL_Wearing_G2, 10.0);
            MyList.Array_Multiply_With(ref BM_SIDL_Wearing_G3, 10.0);
            MyList.Array_Multiply_With(ref BM_SIDL_Wearing_G4, 10.0);





            #endregion Bending Moment


            #region Cross Girder Data


            double crss_hog, crss_sag, crss_frc;

            _cross_joints.Clear();

            for (i = 0; i < jn_col.Count; i++)
            {
                _cross_joints.Add(jn_col[i].NodeNo);
            }


            List<double> lst_crss_hog = new List<double>();
            List<double> lst_crss_sag = new List<double>();
            List<double> lst_crss_frc = new List<double>();

            crss_hog = Minor_Analysis.TotalLoad_Analysis.GetJoint_Max_Hogging(_cross_joints, true);
            crss_sag = Minor_Analysis.TotalLoad_Analysis.GetJoint_Max_Sagging(_cross_joints, true);
            crss_frc = Minor_Analysis.TotalLoad_Analysis.GetJoint_ShearForce(_cross_joints);

            lst_crss_hog.Add(crss_hog);
            lst_crss_sag.Add(crss_sag);
            lst_crss_frc.Add(crss_frc);


            if (all_loads.Count > 0)
            {
                for (i = 0; i < Minor_Analysis.All_LL_Analysis.Count; i++)
                {
                    crss_hog = Minor_Analysis.All_LL_Analysis[i].GetJoint_Max_Hogging(_cross_joints, true);
                    crss_sag = Minor_Analysis.All_LL_Analysis[i].GetJoint_Max_Sagging(_cross_joints, true);
                    crss_frc = Minor_Analysis.All_LL_Analysis[i].GetJoint_ShearForce(_cross_joints);

                    lst_crss_hog.Add(crss_hog);
                    lst_crss_sag.Add(crss_sag);
                    lst_crss_frc.Add(crss_frc);

                }
            }
            else
            {
                crss_hog = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Hogging(_cross_joints, true);
                crss_sag = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Sagging(_cross_joints, true);
                crss_frc = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_ShearForce(_cross_joints);

                lst_crss_hog.Add(crss_hog);
                lst_crss_sag.Add(crss_sag);
                lst_crss_frc.Add(crss_frc);


                crss_hog = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Hogging(_cross_joints, true);
                crss_sag = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Sagging(_cross_joints, true);
                crss_frc = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_ShearForce(_cross_joints);

                lst_crss_hog.Add(crss_hog);
                lst_crss_sag.Add(crss_sag);
                lst_crss_frc.Add(crss_frc);


                crss_hog = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Hogging(_cross_joints, true);
                crss_sag = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Sagging(_cross_joints, true);
                crss_frc = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_ShearForce(_cross_joints);

                lst_crss_hog.Add(crss_hog);
                lst_crss_sag.Add(crss_sag);
                lst_crss_frc.Add(crss_frc);


                crss_hog = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Hogging(_cross_joints, true);
                crss_sag = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Sagging(_cross_joints, true);
                crss_frc = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_ShearForce(_cross_joints);

                lst_crss_hog.Add(crss_hog);
                lst_crss_sag.Add(crss_sag);
                lst_crss_frc.Add(crss_frc);


                crss_hog = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Hogging(_cross_joints, true);
                crss_sag = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Sagging(_cross_joints, true);
                crss_frc = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_ShearForce(_cross_joints);

                lst_crss_hog.Add(crss_hog);
                lst_crss_sag.Add(crss_sag);
                lst_crss_frc.Add(crss_frc);


                crss_hog = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Hogging(_cross_joints, true);
                crss_sag = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Sagging(_cross_joints, true);
                crss_frc = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_ShearForce(_cross_joints);

                lst_crss_hog.Add(crss_hog);
                lst_crss_sag.Add(crss_sag);
                lst_crss_frc.Add(crss_frc);
            }

            lst_crss_hog.Sort();
            lst_crss_hog.Reverse();
            lst_crss_sag.Sort();
            //lst_crss_sag.Reverse();
            lst_crss_frc.Sort();
            lst_crss_frc.Reverse();

            if (dgv_cross_user_input.RowCount > 7)
            {
                dgv_cross_user_input[1, dgv_cross_user_input.RowCount - 1].Value = Math.Abs(lst_crss_frc[0]).ToString("f3");
                dgv_cross_user_input[1, dgv_cross_user_input.RowCount - 2].Value = Math.Abs(lst_crss_sag[0]).ToString("f3");
                dgv_cross_user_input[1, dgv_cross_user_input.RowCount - 3].Value = Math.Abs(lst_crss_hog[0]).ToString("f3");
            }
            #endregion Cross Girder Data

            val = 0.0;


            string format = "{0,-30} {1,12} {2,10:f3} {3,15:f3} {4,10:f3} {5,10:f3} {6,10:f3} {7,10:f3}";

            Result.Add(string.Format(""));

            #region Summary 1

            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------"));
            Result.Add(string.Format("RCC T GIRDER (LIMIT STATE METHOD) ANALYSIS RESULTS "));
            Result.Add(string.Format("---------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of BM and SF for different load cases ( Unfactored)"));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of B.M. & S.F. due to Dead Load (Forces due to Self weight of girder) kN-m"));
            Result.Add(string.Format("------------------------------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("     Girder No                  Components    Support       Web widening    1/8th     1/4th      3/8th        Mid "));
            Result.Add(string.Format("                                                                 end        span      span       span        span"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            
            
            Result.Add(string.Format(format, "G1 ( Analysis Long Member )", "BM in kN-m", 
                                                BM_DL_Self_Weight_G1[0],
                                                BM_DL_Self_Weight_G1[1],
                                                BM_DL_Self_Weight_G1[2],
                                                BM_DL_Self_Weight_G1[3],
                                                BM_DL_Self_Weight_G1[4],
                                                BM_DL_Self_Weight_G1[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN", SF_DL_Self_Weight_G1[0],
                                                SF_DL_Self_Weight_G1[1],
                                                SF_DL_Self_Weight_G1[2],
                                                SF_DL_Self_Weight_G1[3],
                                                SF_DL_Self_Weight_G1[4],
                                                SF_DL_Self_Weight_G1[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G2 ( Analysis Long Member )", "BM in kN-m", 
                                                BM_DL_Self_Weight_G2[0],
                                                BM_DL_Self_Weight_G2[1],
                                                BM_DL_Self_Weight_G2[2],
                                                BM_DL_Self_Weight_G2[3],
                                                BM_DL_Self_Weight_G2[4],
                                                BM_DL_Self_Weight_G2[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN", 
                                                SF_DL_Self_Weight_G2[0],
                                                SF_DL_Self_Weight_G2[1],
                                                SF_DL_Self_Weight_G2[2],
                                                SF_DL_Self_Weight_G2[3],
                                                SF_DL_Self_Weight_G2[4],
                                                SF_DL_Self_Weight_G2[5]));

            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            #endregion Summary 1

            #region Summary 2

            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of B.M. & S.F. due to Dead Load (Forces due to Deck slab Wet concrete and Shuttering load)"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("     Girder No                  Components    Support       Web widening    1/8th     1/4th      3/8th        Mid "));
            Result.Add(string.Format("                                                                 end        span      span       span        span"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(format, "G1 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Deck_Wet_Conc_G1[0],
                                                BM_DL_Deck_Wet_Conc_G1[1],
                                                BM_DL_Deck_Wet_Conc_G1[2],
                                                BM_DL_Deck_Wet_Conc_G1[3],
                                                BM_DL_Deck_Wet_Conc_G1[4],
                                                BM_DL_Deck_Wet_Conc_G1[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Deck_Wet_Conc_G1[0],
                                                SF_DL_Deck_Wet_Conc_G1[1],
                                                SF_DL_Deck_Wet_Conc_G1[2],
                                                SF_DL_Deck_Wet_Conc_G1[3],
                                                SF_DL_Deck_Wet_Conc_G1[4],
                                                SF_DL_Deck_Wet_Conc_G1[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G2 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Deck_Wet_Conc_G2[0],
                                                BM_DL_Deck_Wet_Conc_G2[1],
                                                BM_DL_Deck_Wet_Conc_G2[2],
                                                BM_DL_Deck_Wet_Conc_G2[3],
                                                BM_DL_Deck_Wet_Conc_G2[4],
                                                BM_DL_Deck_Wet_Conc_G2[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Deck_Wet_Conc_G2[0],
                                                SF_DL_Deck_Wet_Conc_G2[1],
                                                SF_DL_Deck_Wet_Conc_G2[2],
                                                SF_DL_Deck_Wet_Conc_G2[3],
                                                SF_DL_Deck_Wet_Conc_G2[4],
                                                SF_DL_Deck_Wet_Conc_G2[5]));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            #endregion Summary 2

            #region Summary 3

            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of B.M. & S.F. due to Deshutering load"));
            Result.Add(string.Format("-----------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("     Girder No                  Components    Support       Web widening    1/8th     1/4th      3/8th        Mid "));
            Result.Add(string.Format("                                                                 end        span      span       span        span"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(format, "G1 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Deck_Dry_Conc_G1[0],
                                                BM_DL_Deck_Dry_Conc_G1[1],
                                                BM_DL_Deck_Dry_Conc_G1[2],
                                                BM_DL_Deck_Dry_Conc_G1[3],
                                                BM_DL_Deck_Dry_Conc_G1[4],
                                                BM_DL_Deck_Dry_Conc_G1[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Deck_Dry_Conc_G1[0],
                                                SF_DL_Deck_Dry_Conc_G1[1],
                                                SF_DL_Deck_Dry_Conc_G1[2],
                                                SF_DL_Deck_Dry_Conc_G1[3],
                                                SF_DL_Deck_Dry_Conc_G1[4],
                                                SF_DL_Deck_Dry_Conc_G1[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G2 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Deck_Dry_Conc_G2[0],
                                                BM_DL_Deck_Dry_Conc_G2[1],
                                                BM_DL_Deck_Dry_Conc_G2[2],
                                                BM_DL_Deck_Dry_Conc_G2[3],
                                                BM_DL_Deck_Dry_Conc_G2[4],
                                                BM_DL_Deck_Dry_Conc_G2[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Deck_Dry_Conc_G2[0],
                                                SF_DL_Deck_Dry_Conc_G2[1],
                                                SF_DL_Deck_Dry_Conc_G2[2],
                                                SF_DL_Deck_Dry_Conc_G2[3],
                                                SF_DL_Deck_Dry_Conc_G2[4],
                                                SF_DL_Deck_Dry_Conc_G2[5]));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            #endregion Summary 3

            #region Summary 4

            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of B.M. & S.F. per girder due to Dead Load (Forces due to Self weight of girder,Deck slab dry concrete)"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("     Girder No                  Components    Support       Web widening    1/8th     1/4th      3/8th        Mid "));
            Result.Add(string.Format("                                                                 end        span      span       span        span"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(format, "G1 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Self_Deck_G1[0],
                                                BM_DL_Self_Deck_G1[1],
                                                BM_DL_Self_Deck_G1[2],
                                                BM_DL_Self_Deck_G1[3],
                                                BM_DL_Self_Deck_G1[4],
                                                BM_DL_Self_Deck_G1[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Self_Deck_G1[0],
                                                SF_DL_Self_Deck_G1[1],
                                                SF_DL_Self_Deck_G1[2],
                                                SF_DL_Self_Deck_G1[3],
                                                SF_DL_Self_Deck_G1[4],
                                                SF_DL_Self_Deck_G1[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G2 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Self_Deck_G2[0],
                                                BM_DL_Self_Deck_G2[1],
                                                BM_DL_Self_Deck_G2[2],
                                                BM_DL_Self_Deck_G2[3],
                                                BM_DL_Self_Deck_G2[4],
                                                BM_DL_Self_Deck_G2[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Self_Deck_G2[0],
                                                SF_DL_Self_Deck_G2[1],
                                                SF_DL_Self_Deck_G2[2],
                                                SF_DL_Self_Deck_G2[3],
                                                SF_DL_Self_Deck_G2[4],
                                                SF_DL_Self_Deck_G2[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G3 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Self_Deck_G3[0],
                                                BM_DL_Self_Deck_G3[1],
                                                BM_DL_Self_Deck_G3[2],
                                                BM_DL_Self_Deck_G3[3],
                                                BM_DL_Self_Deck_G3[4],
                                                BM_DL_Self_Deck_G3[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Self_Deck_G3[0],
                                                SF_DL_Self_Deck_G3[1],
                                                SF_DL_Self_Deck_G3[2],
                                                SF_DL_Self_Deck_G3[3],
                                                SF_DL_Self_Deck_G3[4],
                                                SF_DL_Self_Deck_G3[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G4 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Self_Deck_G4[0],
                                                BM_DL_Self_Deck_G4[1],
                                                BM_DL_Self_Deck_G4[2],
                                                BM_DL_Self_Deck_G4[3],
                                                BM_DL_Self_Deck_G4[4],
                                                BM_DL_Self_Deck_G4[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Self_Deck_G4[0],
                                                SF_DL_Self_Deck_G4[1],
                                                SF_DL_Self_Deck_G4[2],
                                                SF_DL_Self_Deck_G4[3],
                                                SF_DL_Self_Deck_G4[4],
                                                SF_DL_Self_Deck_G4[5]));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            #endregion Summary 4

            #region Summary 5

            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of B.M. & S.F. per girder due to SIDL(Crash barrier)"));
            Result.Add(string.Format("------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("     Girder No                  Components    Support       Web widening    1/8th     1/4th      3/8th        Mid "));
            Result.Add(string.Format("                                                                 end        span      span       span        span"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(format, "G1 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Crash_Barrier_G1[0],
                                                BM_SIDL_Crash_Barrier_G1[1],
                                                BM_SIDL_Crash_Barrier_G1[2],
                                                BM_SIDL_Crash_Barrier_G1[3],
                                                BM_SIDL_Crash_Barrier_G1[4],
                                                BM_SIDL_Crash_Barrier_G1[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Crash_Barrier_G1[0],
                                                SF_SIDL_Crash_Barrier_G1[1],
                                                SF_SIDL_Crash_Barrier_G1[2],
                                                SF_SIDL_Crash_Barrier_G1[3],
                                                SF_SIDL_Crash_Barrier_G1[4],
                                                SF_SIDL_Crash_Barrier_G1[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G2 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Crash_Barrier_G2[0],
                                                BM_SIDL_Crash_Barrier_G2[1],
                                                BM_SIDL_Crash_Barrier_G2[2],
                                                BM_SIDL_Crash_Barrier_G2[3],
                                                BM_SIDL_Crash_Barrier_G2[4],
                                                BM_SIDL_Crash_Barrier_G2[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Crash_Barrier_G2[0],
                                                SF_SIDL_Crash_Barrier_G2[1],
                                                SF_SIDL_Crash_Barrier_G2[2],
                                                SF_SIDL_Crash_Barrier_G2[3],
                                                SF_SIDL_Crash_Barrier_G2[4],
                                                SF_SIDL_Crash_Barrier_G2[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G3 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Crash_Barrier_G3[0],
                                                BM_SIDL_Crash_Barrier_G3[1],
                                                BM_SIDL_Crash_Barrier_G3[2],
                                                BM_SIDL_Crash_Barrier_G3[3],
                                                BM_SIDL_Crash_Barrier_G3[4],
                                                BM_SIDL_Crash_Barrier_G3[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Crash_Barrier_G3[0],
                                                SF_SIDL_Crash_Barrier_G3[1],
                                                SF_SIDL_Crash_Barrier_G3[2],
                                                SF_SIDL_Crash_Barrier_G3[3],
                                                SF_SIDL_Crash_Barrier_G3[4],
                                                SF_SIDL_Crash_Barrier_G3[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G4 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Crash_Barrier_G4[0],
                                                BM_SIDL_Crash_Barrier_G4[1],
                                                BM_SIDL_Crash_Barrier_G4[2],
                                                BM_SIDL_Crash_Barrier_G4[3],
                                                BM_SIDL_Crash_Barrier_G4[4],
                                                BM_SIDL_Crash_Barrier_G4[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Crash_Barrier_G4[0],
                                                SF_SIDL_Crash_Barrier_G4[1],
                                                SF_SIDL_Crash_Barrier_G4[2],
                                                SF_SIDL_Crash_Barrier_G4[3],
                                                SF_SIDL_Crash_Barrier_G4[4],
                                                SF_SIDL_Crash_Barrier_G4[5]));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            #endregion Summary 5

            #region Summary 6

            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of B.M. & S.F. per girder due to SIDL(Wearing coat)"));
            Result.Add(string.Format("------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("     Girder No                  Components    Support       Web widening    1/8th     1/4th      3/8th        Mid "));
            Result.Add(string.Format("                                                                 end        span      span       span        span"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(format, "G1 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Wearing_G1[0],
                                                BM_SIDL_Wearing_G1[1],
                                                BM_SIDL_Wearing_G1[2],
                                                BM_SIDL_Wearing_G1[3],
                                                BM_SIDL_Wearing_G1[4],
                                                BM_SIDL_Wearing_G1[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Wearing_G1[0],
                                                SF_SIDL_Wearing_G1[1],
                                                SF_SIDL_Wearing_G1[2],
                                                SF_SIDL_Wearing_G1[3],
                                                SF_SIDL_Wearing_G1[4],
                                                SF_SIDL_Wearing_G1[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G2 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Wearing_G2[0],
                                                BM_SIDL_Wearing_G2[1],
                                                BM_SIDL_Wearing_G2[2],
                                                BM_SIDL_Wearing_G2[3],
                                                BM_SIDL_Wearing_G2[4],
                                                BM_SIDL_Wearing_G2[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Wearing_G2[0],
                                                SF_SIDL_Wearing_G2[1],
                                                SF_SIDL_Wearing_G2[2],
                                                SF_SIDL_Wearing_G2[3],
                                                SF_SIDL_Wearing_G2[4],
                                                SF_SIDL_Wearing_G2[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G3 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Wearing_G3[0],
                                                BM_SIDL_Wearing_G3[1],
                                                BM_SIDL_Wearing_G3[2],
                                                BM_SIDL_Wearing_G3[3],
                                                BM_SIDL_Wearing_G3[4],
                                                BM_SIDL_Wearing_G3[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Wearing_G3[0],
                                                SF_SIDL_Wearing_G3[1],
                                                SF_SIDL_Wearing_G3[2],
                                                SF_SIDL_Wearing_G3[3],
                                                SF_SIDL_Wearing_G3[4],
                                                SF_SIDL_Wearing_G3[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G4 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Wearing_G4[0],
                                                BM_SIDL_Wearing_G4[1],
                                                BM_SIDL_Wearing_G4[2],
                                                BM_SIDL_Wearing_G4[3],
                                                BM_SIDL_Wearing_G4[4],
                                                BM_SIDL_Wearing_G4[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Wearing_G4[0],
                                                SF_SIDL_Wearing_G4[1],
                                                SF_SIDL_Wearing_G4[2],
                                                SF_SIDL_Wearing_G4[3],
                                                SF_SIDL_Wearing_G4[4],
                                                SF_SIDL_Wearing_G4[5]));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            #endregion Summary 6

            #region Summary 7 

            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of B.M. & S.F. per girder due to Live Load"));
            Result.Add(string.Format("---------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("     Girder No                  Components    Support       Web widening    1/8th     1/4th      3/8th        Mid "));
            Result.Add(string.Format("                                                                 end        span      span       span        span"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            Minor_Analysis.Live_Load_List = LoadData.GetLiveLoads(MyList.Get_LL_TXT_File(Minor_Analysis.TotalAnalysis_Input_File));

            //Result.Add(string.Format(format, "LL1(1 Lane TYPE 3)", "BM in kN-m",




            for (i = 0; i < BM_LL_List.Count; i++)
            {
                BM_LL_1 = BM_LL_List[i];
                SF_LL_1 = SF_LL_List[i];


                //Result.Add(string.Format(format, "LL1( 1L " + Long_Girder_Analysis.Live_Load_List[2].Code + " )", "BM in kN-m",
                Result.Add(string.Format(format, "Live Load Analysis " + (i + 1) + "", "BM in kN-m",
                                                    BM_LL_1[0],
                                                    BM_LL_1[1],
                                                    BM_LL_1[2],
                                                    BM_LL_1[3],
                                                    BM_LL_1[4],
                                                    BM_LL_1[5]));
                //Result.Add(string.Format(""));
                Result.Add(string.Format(format, "", "SF in kN",
                                                    SF_LL_1[0],
                                                    SF_LL_1[1],
                                                    SF_LL_1[2],
                                                    SF_LL_1[3],
                                                    SF_LL_1[4],
                                                    SF_LL_1[5]));

                Result.Add(string.Format(""));
            }
            if (false)
            {
                #region Live Load

                Result.Add(string.Format(format, "LL1( 1L " + Minor_Analysis.Live_Load_List[2].Code + " )", "BM in kN-m",
                                                    BM_LL_1[0],
                                                    BM_LL_1[1],
                                                    BM_LL_1[2],
                                                    BM_LL_1[3],
                                                    BM_LL_1[4],
                                                    BM_LL_1[5]));
                //Result.Add(string.Format(""));
                Result.Add(string.Format(format, "", "SF in kN",
                                                    SF_LL_1[0],
                                                    SF_LL_1[1],
                                                    SF_LL_1[2],
                                                    SF_LL_1[3],
                                                    SF_LL_1[4],
                                                    SF_LL_1[5]));
                Result.Add(string.Format(""));







                //Result.Add(string.Format(format, "LL2(1 Lane TYPE 1)", "BM in kN-m",
                Result.Add(string.Format(format, "LL2( 1L " + Minor_Analysis.Live_Load_List[0].Code + " )", "BM in kN-m",
                                                   BM_LL_2[0],
                                                   BM_LL_2[1],
                                                   BM_LL_2[2],
                                                   BM_LL_2[3],
                                                   BM_LL_2[4],
                                                   BM_LL_2[5]));
                //Result.Add(string.Format(""));
                Result.Add(string.Format(format, "", "SF in kN",
                                                    SF_LL_2[0],
                                                    SF_LL_2[1],
                                                    SF_LL_2[2],
                                                    SF_LL_2[3],
                                                    SF_LL_2[4],
                                                    SF_LL_2[5]));
                Result.Add(string.Format(""));
                //Result.Add(string.Format(format, "LL3(2 Lane TYPE 1)", "BM in kN-m",
                Result.Add(string.Format(format, "LL3( 2L " + Minor_Analysis.Live_Load_List[0].Code + " )", "BM in kN-m",
                                                   BM_LL_3[0],
                                                   BM_LL_3[1],
                                                   BM_LL_3[2],
                                                   BM_LL_3[3],
                                                   BM_LL_3[4],
                                                   BM_LL_3[5]));
                //Result.Add(string.Format(""));
                Result.Add(string.Format(format, "", "SF in kN",
                                                    SF_LL_3[0],
                                                    SF_LL_3[1],
                                                    SF_LL_3[2],
                                                    SF_LL_3[3],
                                                    SF_LL_3[4],
                                                    SF_LL_3[5]));
                Result.Add(string.Format(""));
                //Result.Add(string.Format(format, "LL4(3 Lane TYPE 1)", "BM in kN-m",
                Result.Add(string.Format(format, "LL4( 3L " + Minor_Analysis.Live_Load_List[0].Code.Replace("IRC", "") + " )", "BM in kN-m",
                                                   BM_LL_4[0],
                                                   BM_LL_4[1],
                                                   BM_LL_4[2],
                                                   BM_LL_4[3],
                                                   BM_LL_4[4],
                                                   BM_LL_4[5]));
                //Result.Add(string.Format(""));
                Result.Add(string.Format(format, "", "SF in kN",
                                                    SF_LL_4[0],
                                                    SF_LL_4[1],
                                                    SF_LL_4[2],
                                                    SF_LL_4[3],
                                                    SF_LL_4[4],
                                                    SF_LL_4[5]));
                Result.Add(string.Format(""));
                //Result.Add(string.Format(format, "LL5(1L TYPE 1 + 1L TYPE 3)", "BM in kN-m",
                Result.Add(string.Format(format, "LL5( 1L " + Minor_Analysis.Live_Load_List[0].Code.Replace("IRC", "") + " + 1L " + Minor_Analysis.Live_Load_List[2].Code.Replace("IRC", "") + " )", "BM in kN-m",
                                                   BM_LL_5[0],
                                                   BM_LL_5[1],
                                                   BM_LL_5[2],
                                                   BM_LL_5[3],
                                                   BM_LL_5[4],
                                                   BM_LL_5[5]));
                //Result.Add(string.Format(""));
                Result.Add(string.Format(format, "", "SF in kN",
                                                    SF_LL_5[0],
                                                    SF_LL_5[1],
                                                    SF_LL_5[2],
                                                    SF_LL_5[3],
                                                    SF_LL_5[4],
                                                    SF_LL_5[5]));
                Result.Add(string.Format(""));
                //Result.Add(string.Format(format, "LL6(1L TYPE 3 + 1L TYPE 1)", "BM in kN-m",
                Result.Add(string.Format(format, "LL6( 1L " + Minor_Analysis.Live_Load_List[2].Code.Replace("IRC", "") + " + 1L " + Minor_Analysis.Live_Load_List[0].Code.Replace("IRC", "") + " )", "BM in kN-m",
                                                   BM_LL_6[0],
                                                   BM_LL_6[1],
                                                   BM_LL_6[2],
                                                   BM_LL_6[3],
                                                   BM_LL_6[4],
                                                   BM_LL_6[5]));
                //Result.Add(string.Format(""));
                Result.Add(string.Format(format, "", "SF in kN",
                                                    SF_LL_6[0],
                                                    SF_LL_6[1],
                                                    SF_LL_6[2],
                                                    SF_LL_6[3],
                                                    SF_LL_6[4],
                                                    SF_LL_6[5]));

                #endregion Live Load
            }
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            #endregion Summary 7



            List<int> outer_joints = new List<int>();
            List<int> outer_joints_right = new List<int>();
            if (_support_inn_joints.Count > 0)
            {
                outer_joints.Add(_support_inn_joints[0]);
                outer_joints_right.Add(_support_inn_joints[_support_inn_joints.Count - 1]);
            }


            if (_L8_inn_joints.Count > 0)
                outer_joints.Add(_L8_inn_joints[0]);
            outer_joints_right.Add(_L8_inn_joints[_L8_inn_joints.Count - 1]);

            if (_3L16_inn_joints.Count > 0)
            {
                outer_joints.Add(_3L16_inn_joints[0]);
                outer_joints_right.Add(_3L16_inn_joints[_3L16_inn_joints.Count - 1]);
            }

            if (_L4_inn_joints.Count > 0)
            {
                outer_joints.Add(_L4_inn_joints[0]);
                outer_joints_right.Add(_L4_inn_joints[_L4_inn_joints.Count - 1]);
            }
            if (_5L16_inn_joints.Count > 0)
            {
                outer_joints.Add(_5L16_inn_joints[0]);
                outer_joints_right.Add(_5L16_inn_joints[_5L16_inn_joints.Count - 1]);
            }
            if (_3L8_inn_joints.Count > 0)
            {
                outer_joints.Add(_3L8_inn_joints[0]);
                outer_joints_right.Add(_3L8_inn_joints[_3L8_inn_joints.Count - 1]);
            }
            if (_7L16_inn_joints.Count > 0)
            {
                outer_joints.Add(_7L16_inn_joints[0]);
                outer_joints_right.Add(_7L16_inn_joints[_7L16_inn_joints.Count - 1]);
            }
            if (_L2_inn_joints.Count > 0)
            {
                outer_joints.Add(_L2_inn_joints[0]);
                outer_joints_right.Add(_L2_inn_joints[_L2_inn_joints.Count - 1]);
            }


            if (_7L16_out_joints.Count > 0)
            {
                outer_joints.Add(_7L16_out_joints[0]);
                outer_joints_right.Add(_7L16_out_joints[_7L16_out_joints.Count - 1]);
            }



            if (_3L8_out_joints.Count > 0)
            {
                outer_joints.Add(_3L8_out_joints[0]);
                outer_joints_right.Add(_3L8_out_joints[_3L8_out_joints.Count - 1]);
            }



            if (_5L16_out_joints.Count > 0)
            {
                outer_joints.Add(_5L16_out_joints[0]);
                outer_joints_right.Add(_5L16_out_joints[_5L16_out_joints.Count - 1]);
            }


            if (_L4_out_joints.Count > 0)
            {
                outer_joints.Add(_L4_out_joints[0]);
                outer_joints_right.Add(_L4_out_joints[_L4_out_joints.Count - 1]);
            }


            if (_3L16_out_joints.Count > 0)
            {
                outer_joints.Add(_3L16_out_joints[0]);
                outer_joints_right.Add(_3L16_out_joints[_3L16_out_joints.Count - 1]);
            }


            if (_L8_out_joints.Count > 0)
            {
                outer_joints.Add(_L8_out_joints[0]);
                outer_joints_right.Add(_L8_out_joints[_L8_out_joints.Count - 1]);
            }


            if (_support_out_joints.Count > 0)
            {
                outer_joints.Add(_support_out_joints[0]);
                outer_joints_right.Add(_support_out_joints[_support_out_joints.Count - 1]);
            }





            //iApp.Progress_ON("Reading Maximum deflection....");


            List<NodeResultData> lst_nrd = new List<NodeResultData>();

            //iApp.SetProgressValue(10, 100);

            lst_nrd.Add(Minor_Analysis.TotalLoad_Analysis.Node_Displacements.Get_Max_Deflection());

            for (i = 0; i < all_loads.Count;i++)
            {
                lst_nrd.Add(Minor_Analysis.All_LL_Analysis[i].Node_Displacements.Get_Max_Deflection());

            }


            //iApp.SetProgressValue(20, 100);
            //lst_nrd.Add(Long_Girder_Analysis.LiveLoad_1_Analysis.Node_Displacements.Get_Max_Deflection());
            //iApp.SetProgressValue(30, 100);
            //lst_nrd.Add(Long_Girder_Analysis.LiveLoad_2_Analysis.Node_Displacements.Get_Max_Deflection());
            //iApp.SetProgressValue(40, 100);
            //lst_nrd.Add(Long_Girder_Analysis.LiveLoad_3_Analysis.Node_Displacements.Get_Max_Deflection());
            //lst_nrd.Add(Long_Girder_Analysis.LiveLoad_4_Analysis.Node_Displacements.Get_Max_Deflection());
            //iApp.SetProgressValue(50, 100);
            //lst_nrd.Add(Long_Girder_Analysis.LiveLoad_5_Analysis.Node_Displacements.Get_Max_Deflection());
            //lst_nrd.Add(Long_Girder_Analysis.LiveLoad_6_Analysis.Node_Displacements.Get_Max_Deflection());
            //iApp.SetProgressValue(60, 100);

                lst_nrd.Add(Minor_Analysis.DeadLoad_Analysis.Node_Displacements.Get_Max_Deflection());


            int max_indx = 0;
            double max_def, allow_def;


            max_def = 0;
            allow_def = (L / 800.0);

            for (i = 0; i < lst_nrd.Count; i++)
            {
                if (max_def < Math.Abs(lst_nrd[i].Max_Translation))
                {
                    max_def = Math.Abs(lst_nrd[i].Max_Translation);
                    max_indx = i;
                }
            }



            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------"));
            Result.Add(string.Format("CHECK FOR LIVE LOAD DEFLECTION"));
            Result.Add(string.Format("---------------------------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(" MAXIMUM     NODE DISPLACEMENTS / ROTATIONS"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(" NODE     LOAD          X-            Y-            Z-             X-               Y-            Z-"));
            Result.Add(string.Format(" NUMBER   CASE    TRANSLATION    TRANSLATION    TRANSLATION     ROTATION        ROTATION      ROTATION"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(lst_nrd[max_indx].ToString()));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));


            Result.Add(string.Format("ALLOWABLE DEFLECTION = SPAN/800 M. = {0}/800 M. = {1} M. ", L, allow_def));
            Result.Add(string.Format(""));
            if (max_def > allow_def)
                Result.Add(string.Format("MAXIMUM  VERTICAL DEFLECTION = {0:f6} M. > {1:f6} M. ", max_def, allow_def));
            else
                Result.Add(string.Format("MAXIMUM  VERTICAL DEFLECTION = {0:f6} M. < {1:f6} M. ", max_def, allow_def));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------"));
            Result.Add(string.Format("CHECK FOR DEAD LOAD DEFLECTION LEFT SIDE OUTER GIRDER"));
            Result.Add(string.Format("---------------------------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(" MAXIMUM     NODE DISPLACEMENTS / ROTATIONS"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(" NODE     LOAD          X-            Y-            Z-             X-               Y-            Z-"));
            Result.Add(string.Format(" NUMBER   CASE    TRANSLATION    TRANSLATION    TRANSLATION     ROTATION        ROTATION      ROTATION"));
            Result.Add(string.Format(""));
            lst_nrd.Clear();
            for (i = 0; i < outer_joints.Count; i++)
            {
                lst_nrd.Add(Minor_Analysis.DeadLoad_Analysis.Node_Displacements.Get_Node_Deflection(outer_joints[i]));
                Result.Add(string.Format(lst_nrd[i].ToString()));
            }
            //iApp.SetProgressValue(70, 100);

            Result.Add(string.Format(""));
            Result.Add(string.Format("ALLOWABLE DEFLECTION = SPAN/800 M. = {0}/800 M. = {1} M. ", L, allow_def));
            Result.Add(string.Format(""));
            Result.Add(string.Format("MAXIMUM NODE DISPLACEMENTS FOR LEFT SIDE OUTER LONG GIRDER"));
            Result.Add(string.Format("----------------------------------------------------------"));
            Result.Add(string.Format(""));
            for (i = 0; i < lst_nrd.Count; i++)
            {
                if (lst_nrd[i].Max_Translation < allow_def)
                    Result.Add(string.Format("MAXIMUM  VERTICAL DISPLACEMENT AT NODE {0}  = {1:f6} M. < {2:f6} M.    OK.", lst_nrd[i].NodeNo, lst_nrd[i].Max_Translation, allow_def));
                else
                    Result.Add(string.Format("MAXIMUM  VERTICAL DISPLACEMENT AT NODE {0}  = {1:f6} M. > {2:f6} M.    NOT OK.", lst_nrd[i].NodeNo, Math.Abs(lst_nrd[i].Max_Translation), allow_def));
            }

            //iApp.SetProgressValue(80, 100);

           
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------"));
            Result.Add(string.Format("CHECK FOR DEAD LOAD DEFLECTION RIGHT SIDE OUTER GIRDER"));
            Result.Add(string.Format("---------------------------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(" MAXIMUM     NODE DISPLACEMENTS / ROTATIONS"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(" NODE     LOAD          X-            Y-            Z-             X-               Y-            Z-"));
            Result.Add(string.Format(" NUMBER   CASE    TRANSLATION    TRANSLATION    TRANSLATION     ROTATION        ROTATION      ROTATION"));
            Result.Add(string.Format(""));
            lst_nrd.Clear();
            for (i = 0; i < outer_joints_right.Count; i++)
            {
                lst_nrd.Add(Minor_Analysis.DeadLoad_Analysis.Node_Displacements.Get_Node_Deflection(outer_joints_right[i]));
                Result.Add(string.Format(lst_nrd[i].ToString()));
            }
            //iApp.SetProgressValue(90, 100);

            Result.Add(string.Format(""));
            Result.Add(string.Format("ALLOWABLE DEFLECTION = SPAN/800 M. = {0}/800 M. = {1} M. ", L, allow_def));
            Result.Add(string.Format(""));
            Result.Add(string.Format("MAXIMUM NODE DISPLACEMENTS FOR RIGHT SIDE OUTER LONG GIRDER"));
            Result.Add(string.Format("------------------------------------------------------------"));
            Result.Add(string.Format(""));
            for (i = 0; i < lst_nrd.Count; i++)
            {
                if (lst_nrd[i].Max_Translation < allow_def)
                    Result.Add(string.Format("MAXIMUM  VERTICAL DISPLACEMENT AT NODE {0}  = {1:f6} M. < {2:f6} M.    OK.", lst_nrd[i].NodeNo, lst_nrd[i].Max_Translation, allow_def));
                else
                    Result.Add(string.Format("MAXIMUM  VERTICAL DISPLACEMENT AT NODE {0}  = {1:f6} M. > {2:f6} M.    NOT OK.", lst_nrd[i].NodeNo, Math.Abs(lst_nrd[i].Max_Translation), allow_def));
            }


            //iApp.SetProgressValue(100, 100);



            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("The Deflection for Dead Load is to be controlled by providing Longitudinal"));
            Result.Add(string.Format("Camber along the length of the Main Girder between end to end supports."));

            //iApp.Progress_OFF();

            Result.Add(string.Format(""));
            rtb_ana_result.Lines = Result.ToArray();

            File.WriteAllLines(File_Long_Girder_Results, Result.ToArray());

            //Long_Girder_Analysis.DeadLoad_Analysis.Node_Displacements

            iApp.RunExe(File_Long_Girder_Results);
            Result.Add(string.Format(""));
        }

        //Chiranjit [2016 03 15]
        void Show_Bearing_Forces()
        {
            
            CBridgeStructure strcs = Minor_Analysis.DeadLoad_Analysis.Analysis;


            MemberCollection mc = new MemberCollection(strcs.Members);

            MemberCollection sort_membs = new MemberCollection();

            JointNodeCollection jn_col = strcs.Joints;

            double L = Minor_Analysis.Length;
            double W = Minor_Analysis.WidthBridge;
            double val = L / 2;
            int i = 0;

            List<int> _support_inn_joints = new List<int>();


            for (i = 0; i < strcs.Supports.Count; i++)
            {
                _support_inn_joints.Add(strcs.Supports[i].NodeNo);
            }
            //val = MyList.StringToDouble(txt_Ana_eff_depth.Text, -999.0);
            val = Deff;

            
            Result.Clear();
            Result.Add("");
 
           
            val = 0.0;


            string format =  "{0,-27} {1,14:f3} {2,18:f3} {3,18:f3} {4,18:f6} {5,18:f6}";

            Result.Clear();

            List<NodeResultData> lst_nrd = new List<NodeResultData>();
            MaxForce mxf = new MaxForce();
            BeamMemberForce bmf_dl = new BeamMemberForce();
            List<MaxForce> lsf_mxf = new List<MaxForce>();

            Reaction_Table rct = new Reaction_Table();
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("MAXIMUM FORCES AND ROTATIONS"));
            Result.Add(string.Format("--------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("-----------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("                                  VERTICAL       TRANS. HORIZONTAL    LONG HORIZONTAL    X-ROTATION         Z-ROTATION    "));
            Result.Add(string.Format("                                    LOAD               FORCE               FORCE"));
            Result.Add(string.Format("-----------------------------------------------------------------------------------------------------------------------"));
            //Reaction_Data rdt = new Reaction_Data();
            

            //List
            #region Get Forces from DL

            lsf_mxf.Clear();

            #region Get Node results from Dead load analysis
            //Get Node results from Dead load analysis
            mxf = Minor_Analysis.DeadLoad_Analysis.GetJoint_R2_Shear(_support_inn_joints, 1);
            //lsf_mxf.Add(mxf);
            rct.DL.Vertival_Load = mxf.Force;

            //Get Node results from Dead load analysis
            mxf = Minor_Analysis.DeadLoad_Analysis.GetJoint_R3_Shear(_support_inn_joints, 1);
            //lsf_mxf.Add(mxf);
            rct.DL.Transverse_Force = mxf.Force;

            //Get Node results from Dead load analysis
            mxf = Minor_Analysis.DeadLoad_Analysis.GetJoint_R1_Axial(_support_inn_joints, 1);
            //lsf_mxf.Add(mxf);
            rct.DL.Long_Force = mxf.Force;

            lst_nrd.Clear();

            #region Get Node results from Dead load analysis
            //Get Node results from Dead load analysis
            NodeResults ndrs = Minor_Analysis.DeadLoad_Analysis.Node_Displacements.Get_NodeResults(1);

            foreach (var item in ndrs)
            {
                if (_support_inn_joints.Contains(item.NodeNo))
                    lst_nrd.Add(item);

            }
            NodeResultData nrd = new NodeResultData();

            #region Get Max Translations Rataions
            foreach (var item in lst_nrd)
            {
                if (Math.Abs(nrd.X_Translation) < Math.Abs(item.X_Translation))
                    nrd.X_Translation = item.X_Translation;
                if (Math.Abs(nrd.Y_Translation) < Math.Abs(item.Y_Translation))
                    nrd.Y_Translation = item.Y_Translation;
                if (Math.Abs(nrd.Z_Translation) < Math.Abs(item.Z_Translation))
                    nrd.Z_Translation = item.Z_Translation;

                if (Math.Abs(nrd.X_Rotation) < Math.Abs(item.X_Rotation))
                    nrd.X_Rotation = item.X_Rotation;
                if (Math.Abs(nrd.Y_Rotation) < Math.Abs(item.Y_Rotation))
                    nrd.Y_Rotation = item.Y_Rotation;
                if (Math.Abs(nrd.Z_Rotation) < Math.Abs(item.Z_Rotation))
                    nrd.Z_Rotation = item.Z_Rotation;
            }

            #endregion Get Max Translations Rataions


            #endregion Get Node results from Dead load analysis

            rct.DL.X_Rotation = nrd.X_Rotation;
            rct.DL.Z_Rotation = nrd.Z_Rotation;


            //Result.Add(string.Format(format, "SIDL CRASH BARRIER", lsf_mxf[0].Force, lsf_mxf[1].Force, lsf_mxf[2].Force));
            Result.Add(string.Format(format, "DEAD LOAD SELF WEIGHT", rct.DL.Vertival_Load,
                rct.DL.Transverse_Force, rct.DL.Long_Force, rct.DL.X_Rotation, rct.DL.Z_Rotation));

            #endregion Get Node results from Dead load analysis



            #endregion Get Forces

            #region Get Forces from SIDL CRASH BARRIER

            lsf_mxf.Clear();

            #region Get Node results from Dead load analysis
            //Get Node results from Dead load analysis
            mxf = Minor_Analysis.DeadLoad_Analysis.GetJoint_R2_Shear(_support_inn_joints, 5);
            //lsf_mxf.Add(mxf);
            rct.SIDL.Vertival_Load = mxf.Force;

            //Get Node results from Dead load analysis
            mxf = Minor_Analysis.DeadLoad_Analysis.GetJoint_R3_Shear(_support_inn_joints, 5);
            //lsf_mxf.Add(mxf);
            rct.SIDL.Transverse_Force = mxf.Force;

            //Get Node results from Dead load analysis
            mxf = Minor_Analysis.DeadLoad_Analysis.GetJoint_R1_Axial(_support_inn_joints, 5);
            //lsf_mxf.Add(mxf);
            rct.SIDL.Long_Force = mxf.Force;





            lst_nrd.Clear();

            #region Get Node results from Dead load analysis
            //Get Node results from Dead load analysis
            ndrs = Minor_Analysis.DeadLoad_Analysis.Node_Displacements.Get_NodeResults(5);

            foreach (var item in ndrs)
            {
                if (_support_inn_joints.Contains(item.NodeNo))
                    lst_nrd.Add(item);

            }
            nrd = new NodeResultData();

            #region Get Max Translations Rataions
            foreach (var item in lst_nrd)
            {
                if (Math.Abs(nrd.X_Translation) < Math.Abs(item.X_Translation))
                    nrd.X_Translation = item.X_Translation;
                if (Math.Abs(nrd.Y_Translation) < Math.Abs(item.Y_Translation))
                    nrd.Y_Translation = item.Y_Translation;
                if (Math.Abs(nrd.Z_Translation) < Math.Abs(item.Z_Translation))
                    nrd.Z_Translation = item.Z_Translation;

                if (Math.Abs(nrd.X_Rotation) < Math.Abs(item.X_Rotation))
                    nrd.X_Rotation = item.X_Rotation;
                if (Math.Abs(nrd.Y_Rotation) < Math.Abs(item.Y_Rotation))
                    nrd.Y_Rotation = item.Y_Rotation;
                if (Math.Abs(nrd.Z_Rotation) < Math.Abs(item.Z_Rotation))
                    nrd.Z_Rotation = item.Z_Rotation;
            }

            #endregion Get Max Translations Rataions


            #endregion Get Node results from Dead load analysis

            rct.SIDL.X_Rotation = nrd.X_Rotation;
            rct.SIDL.Z_Rotation = nrd.Z_Rotation;


            //Result.Add(string.Format(format, "SIDL CRASH BARRIER", lsf_mxf[0].Force, lsf_mxf[1].Force, lsf_mxf[2].Force));
            Result.Add(string.Format(format, "SIDL CRASH BARRIER", rct.SIDL.Vertival_Load,
                rct.SIDL.Transverse_Force,
                rct.SIDL.Long_Force,
                rct.SIDL.X_Rotation,
                rct.SIDL.Z_Rotation));




            #endregion Get Node results from Dead load analysis



            #endregion Get Forces SIDL CRASH BARRIER

            #region Get Forces from SIDL CRASH BARRIER

            lsf_mxf.Clear();

            #region Get Node results from Dead load analysis
            //Get Node results from Dead load analysis
            mxf = Minor_Analysis.DeadLoad_Analysis.GetJoint_R2_Shear(_support_inn_joints, 6);
            //lsf_mxf.Add(mxf);
            rct[2].Vertival_Load = mxf.Force;

            //Get Node results from Dead load analysis
            mxf = Minor_Analysis.DeadLoad_Analysis.GetJoint_R3_Shear(_support_inn_joints, 6);
            //lsf_mxf.Add(mxf);
            rct[2].Transverse_Force = mxf.Force;

            //Get Node results from Dead load analysis
            mxf = Minor_Analysis.DeadLoad_Analysis.GetJoint_R1_Axial(_support_inn_joints, 6);
            //lsf_mxf.Add(mxf);
            rct[2].Long_Force = mxf.Force;





            lst_nrd.Clear();

            #region Get Node results from Dead load analysis
            //Get Node results from Dead load analysis
            ndrs = Minor_Analysis.DeadLoad_Analysis.Node_Displacements.Get_NodeResults(6);

            foreach (var item in ndrs)
            {
                if (_support_inn_joints.Contains(item.NodeNo))
                    lst_nrd.Add(item);

            }
            nrd = new NodeResultData();

            #region Get Max Translations Rataions
            foreach (var item in lst_nrd)
            {
                if (Math.Abs(nrd.X_Translation) < Math.Abs(item.X_Translation))
                    nrd.X_Translation = item.X_Translation;
                if (Math.Abs(nrd.Y_Translation) < Math.Abs(item.Y_Translation))
                    nrd.Y_Translation = item.Y_Translation;
                if (Math.Abs(nrd.Z_Translation) < Math.Abs(item.Z_Translation))
                    nrd.Z_Translation = item.Z_Translation;

                if (Math.Abs(nrd.X_Rotation) < Math.Abs(item.X_Rotation))
                    nrd.X_Rotation = item.X_Rotation;
                if (Math.Abs(nrd.Y_Rotation) < Math.Abs(item.Y_Rotation))
                    nrd.Y_Rotation = item.Y_Rotation;
                if (Math.Abs(nrd.Z_Rotation) < Math.Abs(item.Z_Rotation))
                    nrd.Z_Rotation = item.Z_Rotation;
            }

            #endregion Get Max Translations Rataions


            #endregion Get Node results from Dead load analysis

            rct[2].X_Rotation = nrd.X_Rotation;
            rct[2].Z_Rotation = nrd.Z_Rotation;


            //Result.Add(string.Format(format, "SIDL CRASH BARRIER", lsf_mxf[0].Force, lsf_mxf[1].Force, lsf_mxf[2].Force));
            Result.Add(string.Format(format, "SIDL WEARING COAT", rct[2].Vertival_Load,
                rct[2].Transverse_Force,
                rct[2].Long_Force,
                rct[2].X_Rotation,
                rct[2].Z_Rotation));


            #endregion Get Node results from Dead load analysis



            #endregion Get Forces SIDL CRASH BARRIER


            //Result.Add(string.Format(format, "SIDL CRASH BARRIER", lsf_mxf[0].Force, lsf_mxf[1].Force, lsf_mxf[2].Force));
            Reaction_Data rdt_dl = new Reaction_Data("TOTAL");

            rdt_dl.Vertival_Load = rct[0].Vertival_Load + rct[1].Vertival_Load + rct[2].Vertival_Load;
            rdt_dl.Transverse_Force = rct[0].Transverse_Force + rct[1].Transverse_Force + rct[2].Transverse_Force;
            rdt_dl.Long_Force = rct[0].Long_Force + rct[1].Long_Force + rct[2].Long_Force;
            rdt_dl.X_Rotation = Math.Abs(rct[0].X_Rotation) + Math.Abs(rct[1].X_Rotation) + Math.Abs(rct[2].X_Rotation);
            rdt_dl.Z_Rotation = Math.Abs(rct[0].Z_Rotation) + Math.Abs(rct[1].Z_Rotation) + Math.Abs(rct[2].Z_Rotation);


            Result.Add(string.Format("-----------------------------------------------------------------------------------------------------------------------"));

            Result.Add(string.Format(format, "TOTAL", rdt_dl.Vertival_Load,
                rdt_dl.Transverse_Force,
                rdt_dl.Long_Force,
                rdt_dl.X_Rotation,
                rdt_dl.Z_Rotation));
            Result.Add(string.Format("-----------------------------------------------------------------------------------------------------------------------"));

            Reaction_Data rdt_max = new Reaction_Data("MAXIMUM LL");
            Reaction_Data rdt_min = new Reaction_Data("MINIMUM LL");

            for (i = 0; i < all_loads.Count; i++)
            {
                #region Get Forces from LL ANALYSIS

                Reaction_Data rdt = new Reaction_Data("LL ANALYSIS " + (i + 1));

                lsf_mxf.Clear();

                #region Get Node results from Dead load analysis
                //Get Node results from Dead load analysis
                mxf = Minor_Analysis.All_LL_Analysis[i].GetJoint_R2_Shear(_support_inn_joints);
                //lsf_mxf.Add(mxf);
                rdt.Vertival_Load = mxf.Force;

                //Get Node results from Dead load analysis
                mxf = Minor_Analysis.All_LL_Analysis[i].GetJoint_R3_Shear(_support_inn_joints);
                //lsf_mxf.Add(mxf);
                rdt.Transverse_Force = mxf.Force;

                //Get Node results from Dead load analysis
                mxf = Minor_Analysis.All_LL_Analysis[i].GetJoint_R1_Axial(_support_inn_joints);
                //lsf_mxf.Add(mxf);
                rdt.Long_Force = mxf.Force;


                lst_nrd.Clear();

                #region Get Node results from Dead load analysis
                //Get Node results from Dead load analysis
                ndrs = Minor_Analysis.All_LL_Analysis[i].Node_Displacements;

                foreach (var item in ndrs)
                {
                    if (_support_inn_joints.Contains(item.NodeNo))
                        lst_nrd.Add(item);

                }
                nrd = new NodeResultData();

                #region Get Max Translations Rataions
                foreach (var item in lst_nrd)
                {
                    if (Math.Abs(nrd.X_Translation) < Math.Abs(item.X_Translation))
                        nrd.X_Translation = item.X_Translation;
                    if (Math.Abs(nrd.Y_Translation) < Math.Abs(item.Y_Translation))
                        nrd.Y_Translation = item.Y_Translation;
                    if (Math.Abs(nrd.Z_Translation) < Math.Abs(item.Z_Translation))
                        nrd.Z_Translation = item.Z_Translation;

                    if (Math.Abs(nrd.X_Rotation) < Math.Abs(item.X_Rotation))
                        nrd.X_Rotation = item.X_Rotation;
                    if (Math.Abs(nrd.Y_Rotation) < Math.Abs(item.Y_Rotation))
                        nrd.Y_Rotation = item.Y_Rotation;
                    if (Math.Abs(nrd.Z_Rotation) < Math.Abs(item.Z_Rotation))
                        nrd.Z_Rotation = item.Z_Rotation;
                }

                #endregion Get Max Translations Rataions


                #endregion Get Node results from Dead load analysis

                rdt.X_Rotation = nrd.X_Rotation;
                rdt.Z_Rotation = nrd.Z_Rotation;


                //Result.Add(string.Format(format, "SIDL CRASH BARRIER", lsf_mxf[0].Force, lsf_mxf[1].Force, lsf_mxf[2].Force));
                Result.Add(string.Format(format, rdt.Title, rdt.Vertival_Load,
                    rdt.Transverse_Force,
                    rdt.Long_Force,
                    rdt.X_Rotation,
                    rdt.Z_Rotation));

                #endregion Get Node results from Dead load analysis


                if (i == 0)
                {

                    rct[3] = rdt;

                    rdt_max.Vertival_Load = Math.Abs(rdt.Vertival_Load);
                    rdt_max.Transverse_Force = Math.Abs(rdt.Transverse_Force);
                    rdt_max.Long_Force = Math.Abs(rdt.Long_Force);
                    rdt_max.X_Rotation = Math.Abs(rdt.X_Rotation);
                    rdt_max.Z_Rotation = Math.Abs(rdt.Z_Rotation);

                    rdt_min.Vertival_Load = Math.Abs(rdt.Vertival_Load);
                    rdt_min.Transverse_Force = Math.Abs(rdt.Transverse_Force);
                    rdt_min.Long_Force = Math.Abs(rdt.Long_Force);
                    rdt_min.X_Rotation = Math.Abs(rdt.X_Rotation);
                    rdt_min.Z_Rotation = Math.Abs(rdt.Z_Rotation);

                }
                else rct.Add(rdt);
                //Get Maximum Force
                if (Math.Abs(rdt_max.Vertival_Load) < Math.Abs(rdt.Vertival_Load))
                {
                    rdt_max.Vertival_Load = Math.Abs(rdt.Vertival_Load);
                }
                if (Math.Abs(rdt_max.Transverse_Force) < Math.Abs(rdt.Transverse_Force))
                {
                    rdt_max.Transverse_Force = Math.Abs(rdt.Transverse_Force);
                }
                if (Math.Abs(rdt_max.Long_Force) < Math.Abs(rdt.Long_Force))
                {
                    rdt_max.Long_Force = Math.Abs(rdt.Long_Force);
                }
                if (Math.Abs(rdt_max.X_Rotation) < Math.Abs(rdt.X_Rotation))
                {
                    rdt_max.X_Rotation = Math.Abs(rdt.X_Rotation);
                }
                if (Math.Abs(rdt_max.Z_Rotation) < Math.Abs(rdt.Z_Rotation))
                {
                    rdt_max.Z_Rotation = Math.Abs(rdt.Z_Rotation);
                }



                //Get Minimum Force
                if (Math.Abs(rdt_min.Vertival_Load) > Math.Abs(rdt.Vertival_Load))
                {
                    rdt_min.Vertival_Load = Math.Abs(rdt.Vertival_Load);
                }
                if (Math.Abs(rdt_min.Transverse_Force) > Math.Abs(rdt.Transverse_Force))
                {
                    rdt_min.Transverse_Force = Math.Abs(rdt.Transverse_Force);
                }
                if (Math.Abs(rdt_min.Long_Force) > Math.Abs(rdt.Long_Force))
                {
                    rdt_min.Long_Force = Math.Abs(rdt.Long_Force);
                }
                if (Math.Abs(rdt_min.X_Rotation) > Math.Abs(rdt.X_Rotation))
                {
                    rdt_min.X_Rotation = Math.Abs(rdt.X_Rotation);
                }
                if (Math.Abs(rdt_min.Z_Rotation) > Math.Abs(rdt.Z_Rotation))
                {
                    rdt_min.Z_Rotation = Math.Abs(rdt.Z_Rotation);
                }

                #endregion Get Forces LL ANALYSIS

            }



            //Result.Add(string.Format(format, "SIDL CRASH BARRIER", lsf_mxf[0].Force, lsf_mxf[1].Force, lsf_mxf[2].Force));
            Result.Add(string.Format("-----------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(format,
                rdt_max.Title,
                rdt_max.Vertival_Load,
                rdt_max.Transverse_Force,
                rdt_max.Long_Force,
                rdt_max.X_Rotation,
                rdt_max.Z_Rotation));


            Result.Add(string.Format(format,
                rdt_min.Title,
                rdt_min.Vertival_Load,
                rdt_min.Transverse_Force,
                rdt_min.Long_Force,
                rdt_min.X_Rotation,
                rdt_min.Z_Rotation));
            //Result.Add(string.Format("--------------------------------------------------------------------------------------------------------------"));



            Result.Add(string.Format("-----------------------------------------------------------------------------------------------------------------------"));

            Reaction_Data rdt_DL_LL = new Reaction_Data("TOTAL DL + SIDL + LL(MAX)");

            rdt_DL_LL.Vertival_Load = rdt_max.Vertival_Load + rdt_dl.Vertival_Load;
            rdt_DL_LL.Transverse_Force = rdt_max.Transverse_Force + rdt_dl.Transverse_Force;
            rdt_DL_LL.Long_Force = rdt_max.Long_Force + rdt_dl.Long_Force;
            rdt_DL_LL.X_Rotation = rdt_max.X_Rotation + rdt_dl.X_Rotation;
            rdt_DL_LL.Z_Rotation = rdt_max.Z_Rotation + rdt_dl.Z_Rotation;



            Result.Add(string.Format(format,
                rdt_DL_LL.Title,
                rdt_DL_LL.Vertival_Load,
                rdt_DL_LL.Transverse_Force,
                rdt_DL_LL.Long_Force,
                rdt_DL_LL.X_Rotation,
                rdt_DL_LL.Z_Rotation));



            rdt_DL_LL = new Reaction_Data("TOTAL DL + SIDL + LL(MIN)");

            rdt_DL_LL.Vertival_Load = rdt_min.Vertival_Load + rdt_dl.Vertival_Load;
            rdt_DL_LL.Transverse_Force = rdt_min.Transverse_Force + rdt_dl.Transverse_Force;
            rdt_DL_LL.Long_Force = rdt_min.Long_Force + rdt_dl.Long_Force;
            rdt_DL_LL.X_Rotation = rdt_min.X_Rotation + rdt_dl.X_Rotation;
            rdt_DL_LL.Z_Rotation = rdt_min.Z_Rotation + rdt_dl.Z_Rotation;



            Result.Add(string.Format(format,
                rdt_DL_LL.Title,
                rdt_DL_LL.Vertival_Load,
                rdt_DL_LL.Transverse_Force,
                rdt_DL_LL.Long_Force,
                rdt_DL_LL.X_Rotation,
                rdt_DL_LL.Z_Rotation));




            Result.Add(string.Format("-----------------------------------------------------------------------------------------------------------------------"));



            if (false)
            {
                #region DD





                lst_nrd.Clear();

                #region Get Node results from Dead load analysis
                //Get Node results from Dead load analysis
                ndrs = Minor_Analysis.DeadLoad_Analysis.Node_Displacements.Get_NodeResults(5);

                foreach (var item in ndrs)
                {
                    if (_support_inn_joints.Contains(item.NodeNo))
                        lst_nrd.Add(item);

                }
                nrd = new NodeResultData();

                #region Get Max Translations Rataions
                foreach (var item in lst_nrd)
                {
                    if (Math.Abs(nrd.X_Translation) < Math.Abs(item.X_Translation))
                        nrd.X_Translation = item.X_Translation;
                    if (Math.Abs(nrd.Y_Translation) < Math.Abs(item.Y_Translation))
                        nrd.Y_Translation = item.Y_Translation;
                    if (Math.Abs(nrd.Z_Translation) < Math.Abs(item.Z_Translation))
                        nrd.Z_Translation = item.Z_Translation;

                    if (Math.Abs(nrd.X_Rotation) < Math.Abs(item.X_Rotation))
                        nrd.X_Rotation = item.X_Rotation;
                    if (Math.Abs(nrd.Y_Rotation) < Math.Abs(item.Y_Rotation))
                        nrd.Y_Rotation = item.Y_Rotation;
                    if (Math.Abs(nrd.Z_Rotation) < Math.Abs(item.Z_Rotation))
                        nrd.Z_Rotation = item.Z_Rotation;
                }

                #endregion Get Max Translations Rataions


                #endregion Get Node results from Dead load analysis


                Result.Add(string.Format("----------------------------------------------------------------------"));
                Result.Add(string.Format(format, "TOTAL LOAD", bmf_dl.M1_Torsion, bmf_dl.R2_Shear, bmf_dl.M2_Bending));

                Result.Add(string.Format("----------------------------------------------------------------------"));


                BeamMemberForce bmf_ll = new BeamMemberForce();
                BeamMemberForce bmf_ll_max = new BeamMemberForce();
                BeamMemberForce bmf_ll_min = new BeamMemberForce();
                #region Read Nodes from Live Load files

                //Read Nodes from Live Load files
                for (i = 0; i < all_loads.Count; i++)
                {
                    #region Get Forces

                    lsf_mxf.Clear();

                    #region Get Node results from Dead load analysis
                    //Get Node results from Dead load analysis
                    mxf = Minor_Analysis.All_LL_Analysis[i].GetJoint_R2_Shear(_support_inn_joints);
                    lsf_mxf.Add(mxf);


                    //Get Node results from Dead load analysis
                    mxf = Minor_Analysis.All_LL_Analysis[i].GetJoint_R3_Shear(_support_inn_joints);
                    lsf_mxf.Add(mxf);

                    //Get Node results from Dead load analysis
                    mxf = Minor_Analysis.All_LL_Analysis[i].GetJoint_R1_Axial(_support_inn_joints);
                    lsf_mxf.Add(mxf);




                    Result.Add(string.Format(format, "LL ANALYSIS " + (i + 1), lsf_mxf[0].Force, lsf_mxf[1].Force, lsf_mxf[2].Force));

                    if (i == 0)
                    {


                        bmf_ll_max.M1_Torsion = lsf_mxf[0].Force;
                        bmf_ll_max.R2_Shear = lsf_mxf[1].Force;
                        bmf_ll_max.M2_Bending = lsf_mxf[2].Force;

                        bmf_ll_min.M1_Torsion = lsf_mxf[0].Force;
                        bmf_ll_min.R2_Shear = lsf_mxf[1].Force;
                        bmf_ll_min.M2_Bending = lsf_mxf[2].Force;

                    }
                    if (Math.Abs(bmf_ll_max.M1_Torsion) < Math.Abs(lsf_mxf[0].Force))
                    {
                        bmf_ll_max.M1_Torsion = lsf_mxf[0].Force;
                    }
                    if (Math.Abs(bmf_ll_max.R2_Shear) < Math.Abs(lsf_mxf[1].Force))
                    {
                        bmf_ll_max.R2_Shear = lsf_mxf[1].Force;
                    }
                    if (Math.Abs(bmf_ll_max.M2_Bending) < Math.Abs(lsf_mxf[2].Force))
                    {
                        bmf_ll_max.M2_Bending = lsf_mxf[2].Force;
                    }


                    if (Math.Abs(bmf_ll_min.M1_Torsion) > Math.Abs(lsf_mxf[0].Force))
                    {
                        bmf_ll_min.M1_Torsion = lsf_mxf[0].Force;
                    }
                    if (Math.Abs(bmf_ll_min.R2_Shear) > Math.Abs(lsf_mxf[1].Force))
                    {
                        bmf_ll_min.R2_Shear = lsf_mxf[1].Force;
                    }
                    if (Math.Abs(bmf_ll_min.M2_Bending) > Math.Abs(lsf_mxf[2].Force))
                    {
                        bmf_ll_min.M2_Bending = lsf_mxf[2].Force;
                    }

                    #endregion Get Node results from Dead load analysis

                    lsf_mxf.Clear();


                    #endregion Get Forces


                }
                #endregion Read Nodes from Live Load files

                Result.Add(string.Format("----------------------------------------------------------------------"));
                Result.Add(string.Format(format, "MAXIMUM LOAD", bmf_ll_max.M1_Torsion, bmf_ll_max.R2_Shear, bmf_ll_max.M2_Bending));
                Result.Add(string.Format(format, "MINIMUM LOAD", bmf_ll_min.M1_Torsion, bmf_ll_min.R2_Shear, bmf_ll_min.M2_Bending));

                Result.Add(string.Format("----------------------------------------------------------------------"));

                bmf_ll.M1_Torsion = bmf_dl.M1_Torsion + bmf_ll_max.M1_Torsion;
                bmf_ll.R2_Shear = bmf_dl.R2_Shear + bmf_ll_max.R2_Shear;
                bmf_ll.M2_Bending = bmf_dl.M2_Bending + bmf_ll_max.M2_Bending;

                Result.Add(string.Format(format, "TOTAL MAXIMUM LOAD", bmf_ll.M1_Torsion, bmf_ll.R2_Shear, bmf_ll.M2_Bending));

                bmf_ll.M1_Torsion = bmf_dl.M1_Torsion + bmf_ll_min.M1_Torsion;
                bmf_ll.R2_Shear = bmf_dl.R2_Shear + bmf_ll_min.R2_Shear;
                bmf_ll.M2_Bending = bmf_dl.M2_Bending + bmf_ll_min.M2_Bending;

                Result.Add(string.Format(format, "TOTAL MINIMUM LOAD", bmf_ll.M1_Torsion, bmf_ll.R2_Shear, bmf_ll.M2_Bending));

                Result.Add(string.Format("----------------------------------------------------------------------"));

                #endregion DD
            }

            Result.Add(string.Format(""));
            //rtb_ana_result.Lines = Result.ToArray();

            string brg_frc = Path.Combine(user_path, "BEARING_ANALYSIS_RESULT.TXT"); ;

            File.WriteAllLines(brg_frc, Result.ToArray());

            iApp.RunExe(brg_frc);
            uC_BRD1.Read_Reactions(brg_frc);
            Result.Clear();
        }
        void Show_Bearing_Forces_2016_03_18()
        {

            CBridgeStructure strcs = Minor_Analysis.DeadLoad_Analysis.Analysis;


            MemberCollection mc = new MemberCollection(strcs.Members);

            MemberCollection sort_membs = new MemberCollection();

            JointNodeCollection jn_col = strcs.Joints;

            double L = Minor_Analysis.Length;
            double W = Minor_Analysis.WidthBridge;
            double val = L / 2;
            int i = 0;

            List<int> _support_inn_joints = new List<int>();


            List<int> _support_out_joints = new List<int>();



            List<double> _X_joints = new List<double>();
            List<double> _Z_joints = new List<double>();

            for (i = 0; i < strcs.Supports.Count; i++)
            {
                _support_inn_joints.Add(strcs.Supports[i].NodeNo);
            }
            //val = MyList.StringToDouble(txt_Ana_eff_depth.Text, -999.0);
            val = Deff;

            List<double> _X_min = new List<double>();
            List<double> _X_max = new List<double>();

            Result.Clear();
            Result.Add("");
            Result.Add("");
            Result.Add("");


            val = 0.0;


            string format = "{0,-30} {1,12} {2,10:f3} {3,15:f3} {4,10:f3} {5,10:f3} {6,10:f3} {7,10:f3}";


            format = "{0,-24} {1,14:E5} {2,14:E5} {3,14:E5} {4,14:E5} {5,14:E5} {6,14:E5}";

            Result.Clear();

            List<NodeResultData> lst_nrd = new List<NodeResultData>();


            //NodeResultData nrd_total_dl = new NodeResultData();

            lst_nrd.Clear();

            #region Get Node results from Dead load analysis
            //Get Node results from Dead load analysis
            NodeResults ndrs = Minor_Analysis.DeadLoad_Analysis.Node_Displacements.Get_NodeResults(1);

            foreach (var item in ndrs)
            {
                if (_support_inn_joints.Contains(item.NodeNo))
                    lst_nrd.Add(item);

            }
            NodeResultData nrd = new NodeResultData();

            #region Get Max Translations Rataions
            foreach (var item in lst_nrd)
            {
                if (Math.Abs(nrd.X_Translation) < Math.Abs(item.X_Translation))
                    nrd.X_Translation = item.X_Translation;
                if (Math.Abs(nrd.Y_Translation) < Math.Abs(item.Y_Translation))
                    nrd.Y_Translation = item.Y_Translation;
                if (Math.Abs(nrd.Z_Translation) < Math.Abs(item.Z_Translation))
                    nrd.Z_Translation = item.Z_Translation;

                if (Math.Abs(nrd.X_Rotation) < Math.Abs(item.X_Rotation))
                    nrd.X_Rotation = item.X_Rotation;
                if (Math.Abs(nrd.Y_Rotation) < Math.Abs(item.Y_Rotation))
                    nrd.Y_Rotation = item.Y_Rotation;
                if (Math.Abs(nrd.Z_Rotation) < Math.Abs(item.Z_Rotation))
                    nrd.Z_Rotation = item.Z_Rotation;
            }

            #endregion Get Max Translations Rataions


            Result.Add(string.Format(" MAXIMUM     NODE DISPLACEMENTS / ROTATIONS"));
            Result.Add(string.Format("---------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("                                 X-            Y-            Z-             X-               Y-            Z-"));
            Result.Add(string.Format("                            TRANSLATION    TRANSLATION    TRANSLATION     ROTATION        ROTATION      ROTATION"));
            Result.Add(string.Format("------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(format, "DEAD LOAD",
                nrd.X_Translation,
                nrd.Y_Translation,
                nrd.Z_Translation,
                nrd.X_Rotation,
                nrd.Y_Rotation,
                nrd.Z_Rotation));

            #endregion Get Node results from Dead load analysis

            lst_nrd.Clear();

            #region Get Node results from Dead load analysis
            //Get Node results from Dead load analysis
            ndrs = Minor_Analysis.DeadLoad_Analysis.Node_Displacements.Get_NodeResults(5);

            foreach (var item in ndrs)
            {
                if (_support_inn_joints.Contains(item.NodeNo))
                    lst_nrd.Add(item);

            }
            nrd = new NodeResultData();

            #region Get Max Translations Rataions


            foreach (var item in lst_nrd)
            {
                if (Math.Abs(nrd.X_Translation) < Math.Abs(item.X_Translation))
                    nrd.X_Translation = item.X_Translation;
                if (Math.Abs(nrd.Y_Translation) < Math.Abs(item.Y_Translation))
                    nrd.Y_Translation = item.Y_Translation;
                if (Math.Abs(nrd.Z_Translation) < Math.Abs(item.Z_Translation))
                    nrd.Z_Translation = item.Z_Translation;

                if (Math.Abs(nrd.X_Rotation) < Math.Abs(item.X_Rotation))
                    nrd.X_Rotation = item.X_Rotation;
                if (Math.Abs(nrd.Y_Rotation) < Math.Abs(item.Y_Rotation))
                    nrd.Y_Rotation = item.Y_Rotation;
                if (Math.Abs(nrd.Z_Rotation) < Math.Abs(item.Z_Rotation))
                    nrd.Z_Rotation = item.Z_Rotation;
            }

            #endregion Get Max Translations Rataions



            Result.Add(string.Format(format, "SIDL CRASH BARRIER",
                nrd.X_Translation,
                nrd.Y_Translation,
                nrd.Z_Translation,
                nrd.X_Rotation,
                nrd.Y_Rotation,
                nrd.Z_Rotation));


            #endregion Get Node results from Dead load analysis


            #region Get Node results from Dead load analysis
            //Get Node results from Dead load analysis
            ndrs = Minor_Analysis.DeadLoad_Analysis.Node_Displacements.Get_NodeResults(6);

            foreach (var item in ndrs)
            {
                if (_support_inn_joints.Contains(item.NodeNo))
                    lst_nrd.Add(item);

            }
            nrd = new NodeResultData();

            #region Get Max Translations Rataions


            foreach (var item in lst_nrd)
            {
                if (Math.Abs(nrd.X_Translation) < Math.Abs(item.X_Translation))
                    nrd.X_Translation = item.X_Translation;
                if (Math.Abs(nrd.Y_Translation) < Math.Abs(item.Y_Translation))
                    nrd.Y_Translation = item.Y_Translation;
                if (Math.Abs(nrd.Z_Translation) < Math.Abs(item.Z_Translation))
                    nrd.Z_Translation = item.Z_Translation;

                if (Math.Abs(nrd.X_Rotation) < Math.Abs(item.X_Rotation))
                    nrd.X_Rotation = item.X_Rotation;
                if (Math.Abs(nrd.Y_Rotation) < Math.Abs(item.Y_Rotation))
                    nrd.Y_Rotation = item.Y_Rotation;
                if (Math.Abs(nrd.Z_Rotation) < Math.Abs(item.Z_Rotation))
                    nrd.Z_Rotation = item.Z_Rotation;
            }

            #endregion Get Max Translations Rataions

            Result.Add(string.Format(format, "SIDL WEARING COAT",
                nrd.X_Translation,
                nrd.Y_Translation,
                nrd.Z_Translation,
                nrd.X_Rotation,
                nrd.Y_Rotation,
                nrd.Z_Rotation));

            #endregion Get Node results from Dead load analysis

            //Result.Add(string.Format("------------------------------------------------------------------------------------------------------------------"));

            //Result.Add(string.Format(format, "TOTAL LOAD",
            //    nrd_total_dl.X_Translation,
            //    nrd_total_dl.Y_Translation,
            //    nrd_total_dl.Z_Translation,
            //    nrd_total_dl.X_Rotation,
            //    nrd_total_dl.Y_Rotation,
            //    nrd_total_dl.Z_Rotation));
            Result.Add(string.Format("------------------------------------------------------------------------------------------------------------------"));

            lst_nrd.Clear();

            #region Read Nodes from Live Load files

            //Read Nodes from Live Load files
            for (i = 0; i < all_loads.Count; i++)
            {
                //lst_nrd.Add(Long_Girder_Analysis.All_LL_Analysis[i].Node_Displacements.Get_Max_Deflection());
                lst_nrd.Clear();
                #region Get Node results from Dead load analysis
                //Get Node results from Dead load analysis
                ndrs = Minor_Analysis.All_LL_Analysis[i].Node_Displacements;

                foreach (var item in ndrs)
                {
                    if (_support_inn_joints.Contains(item.NodeNo))
                        lst_nrd.Add(item);

                }
                nrd = new NodeResultData();

                #region Get Max Translations Rataions


                foreach (var item in lst_nrd)
                {
                    if (Math.Abs(nrd.X_Translation) < Math.Abs(item.X_Translation))
                        nrd.X_Translation = item.X_Translation;
                    if (Math.Abs(nrd.Y_Translation) < Math.Abs(item.Y_Translation))
                        nrd.Y_Translation = item.Y_Translation;
                    if (Math.Abs(nrd.Z_Translation) < Math.Abs(item.Z_Translation))
                        nrd.Z_Translation = item.Z_Translation;

                    if (Math.Abs(nrd.X_Rotation) < Math.Abs(item.X_Rotation))
                        nrd.X_Rotation = item.X_Rotation;
                    if (Math.Abs(nrd.Y_Rotation) < Math.Abs(item.Y_Rotation))
                        nrd.Y_Rotation = item.Y_Rotation;
                    if (Math.Abs(nrd.Z_Rotation) < Math.Abs(item.Z_Rotation))
                        nrd.Z_Rotation = item.Z_Rotation;
                }

                #endregion Get Max Translations Rataions

                Result.Add(string.Format(format, "LL ANALYSIS " + (i + 1).ToString(),
                    nrd.X_Translation,
                    nrd.Y_Translation,
                    nrd.Z_Translation,
                    nrd.X_Rotation,
                    nrd.Y_Rotation,
                    nrd.Z_Rotation));
                #endregion Get Node results from Dead load analysis

            }
            #endregion Read Nodes from Live Load files
            Result.Add(string.Format("------------------------------------------------------------------------------------------------------------------"));

            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("MAXIMUM FORCES"));
            Result.Add(string.Format("--------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format("----------------------------------------------------------------------"));
            Result.Add(string.Format("                         VERTICAL   TRANS. HORIZONTAL  LONG HORIZONTAL"));
            Result.Add(string.Format("                           LOAD          FORCE            FORCE"));
            Result.Add(string.Format("----------------------------------------------------------------------"));

            MaxForce mxf = new MaxForce();

            BeamMemberForce bmf_dl = new BeamMemberForce();



            List<MaxForce> lsf_mxf = new List<MaxForce>();

            format = "{0,-24} {1,14:E5} {2,14:E5} {3,14:E5}";

            #region Get Forces

            lsf_mxf.Clear();

            #region Get Node results from Dead load analysis
            //Get Node results from Dead load analysis
            mxf = Minor_Analysis.DeadLoad_Analysis.GetJoint_R2_Shear(_support_inn_joints, 1);
            lsf_mxf.Add(mxf);


            //Get Node results from Dead load analysis
            mxf = Minor_Analysis.DeadLoad_Analysis.GetJoint_R3_Shear(_support_inn_joints, 1);
            lsf_mxf.Add(mxf);

            //Get Node results from Dead load analysis
            mxf = Minor_Analysis.DeadLoad_Analysis.GetJoint_R1_Axial(_support_inn_joints, 1);
            lsf_mxf.Add(mxf);

            Result.Add(string.Format(format, "DEAD LOAD", lsf_mxf[0].Force, lsf_mxf[1].Force, lsf_mxf[2].Force));

            bmf_dl.M1_Torsion += lsf_mxf[0].Force;
            bmf_dl.R2_Shear += lsf_mxf[1].Force;
            bmf_dl.M2_Bending += lsf_mxf[2].Force;
            #endregion Get Node results from Dead load analysis

            lsf_mxf.Clear();


            #endregion Get Forces

            #region Get Forces

            lsf_mxf.Clear();

            #region Get Node results from Dead load analysis
            //Get Node results from Dead load analysis
            mxf = Minor_Analysis.DeadLoad_Analysis.GetJoint_R2_Shear(_support_inn_joints, 5);
            lsf_mxf.Add(mxf);


            //Get Node results from Dead load analysis
            mxf = Minor_Analysis.DeadLoad_Analysis.GetJoint_R3_Shear(_support_inn_joints, 5);
            lsf_mxf.Add(mxf);

            //Get Node results from Dead load analysis
            mxf = Minor_Analysis.DeadLoad_Analysis.GetJoint_R1_Axial(_support_inn_joints, 5);
            lsf_mxf.Add(mxf);

            Result.Add(string.Format(format, "SIDL CRASH BARRIER", lsf_mxf[0].Force, lsf_mxf[1].Force, lsf_mxf[2].Force));

            bmf_dl.M1_Torsion += lsf_mxf[0].Force;
            bmf_dl.R2_Shear += lsf_mxf[1].Force;
            bmf_dl.M2_Bending += lsf_mxf[2].Force;
            #endregion Get Node results from Dead load analysis

            lsf_mxf.Clear();


            #endregion Get Forces

            #region Get Forces

            lsf_mxf.Clear();

            #region Get Node results from Dead load analysis
            //Get Node results from Dead load analysis
            mxf = Minor_Analysis.DeadLoad_Analysis.GetJoint_R2_Shear(_support_inn_joints, 6);
            lsf_mxf.Add(mxf);


            //Get Node results from Dead load analysis
            mxf = Minor_Analysis.DeadLoad_Analysis.GetJoint_R3_Shear(_support_inn_joints, 6);
            lsf_mxf.Add(mxf);

            //Get Node results from Dead load analysis
            mxf = Minor_Analysis.DeadLoad_Analysis.GetJoint_R1_Axial(_support_inn_joints, 6);
            lsf_mxf.Add(mxf);
            Result.Add(string.Format(format, "SIDL WEARING COAT", lsf_mxf[0].Force, lsf_mxf[1].Force, lsf_mxf[2].Force));

            bmf_dl.M1_Torsion += lsf_mxf[0].Force;
            bmf_dl.R2_Shear += lsf_mxf[1].Force;
            bmf_dl.M2_Bending += lsf_mxf[2].Force;
            #endregion Get Node results from Dead load analysis

            lsf_mxf.Clear();


            #endregion Get Forces

            Result.Add(string.Format("----------------------------------------------------------------------"));
            Result.Add(string.Format(format, "TOTAL LOAD", bmf_dl.M1_Torsion, bmf_dl.R2_Shear, bmf_dl.M2_Bending));

            Result.Add(string.Format("----------------------------------------------------------------------"));


            BeamMemberForce bmf_ll = new BeamMemberForce();
            BeamMemberForce bmf_ll_max = new BeamMemberForce();
            BeamMemberForce bmf_ll_min = new BeamMemberForce();
            #region Read Nodes from Live Load files

            //Read Nodes from Live Load files
            for (i = 0; i < all_loads.Count; i++)
            {
                #region Get Forces

                lsf_mxf.Clear();

                #region Get Node results from Dead load analysis
                //Get Node results from Dead load analysis
                mxf = Minor_Analysis.All_LL_Analysis[i].GetJoint_R2_Shear(_support_inn_joints);
                lsf_mxf.Add(mxf);


                //Get Node results from Dead load analysis
                mxf = Minor_Analysis.All_LL_Analysis[i].GetJoint_R3_Shear(_support_inn_joints);
                lsf_mxf.Add(mxf);

                //Get Node results from Dead load analysis
                mxf = Minor_Analysis.All_LL_Analysis[i].GetJoint_R1_Axial(_support_inn_joints);
                lsf_mxf.Add(mxf);




                Result.Add(string.Format(format, "LL ANALYSIS " + (i + 1), lsf_mxf[0].Force, lsf_mxf[1].Force, lsf_mxf[2].Force));

                if (i == 0)
                {


                    bmf_ll_max.M1_Torsion = lsf_mxf[0].Force;
                    bmf_ll_max.R2_Shear = lsf_mxf[1].Force;
                    bmf_ll_max.M2_Bending = lsf_mxf[2].Force;

                    bmf_ll_min.M1_Torsion = lsf_mxf[0].Force;
                    bmf_ll_min.R2_Shear = lsf_mxf[1].Force;
                    bmf_ll_min.M2_Bending = lsf_mxf[2].Force;

                }
                if (Math.Abs(bmf_ll_max.M1_Torsion) < Math.Abs(lsf_mxf[0].Force))
                {
                    bmf_ll_max.M1_Torsion = lsf_mxf[0].Force;
                }
                if (Math.Abs(bmf_ll_max.R2_Shear) < Math.Abs(lsf_mxf[1].Force))
                {
                    bmf_ll_max.R2_Shear = lsf_mxf[1].Force;
                }
                if (Math.Abs(bmf_ll_max.M2_Bending) < Math.Abs(lsf_mxf[2].Force))
                {
                    bmf_ll_max.M2_Bending = lsf_mxf[2].Force;
                }


                if (Math.Abs(bmf_ll_min.M1_Torsion) > Math.Abs(lsf_mxf[0].Force))
                {
                    bmf_ll_min.M1_Torsion = lsf_mxf[0].Force;
                }
                if (Math.Abs(bmf_ll_min.R2_Shear) > Math.Abs(lsf_mxf[1].Force))
                {
                    bmf_ll_min.R2_Shear = lsf_mxf[1].Force;
                }
                if (Math.Abs(bmf_ll_min.M2_Bending) > Math.Abs(lsf_mxf[2].Force))
                {
                    bmf_ll_min.M2_Bending = lsf_mxf[2].Force;
                }

                #endregion Get Node results from Dead load analysis

                lsf_mxf.Clear();


                #endregion Get Forces


            }
            #endregion Read Nodes from Live Load files

            Result.Add(string.Format("----------------------------------------------------------------------"));
            Result.Add(string.Format(format, "MAXIMUM LOAD", bmf_ll_max.M1_Torsion, bmf_ll_max.R2_Shear, bmf_ll_max.M2_Bending));
            Result.Add(string.Format(format, "MINIMUM LOAD", bmf_ll_min.M1_Torsion, bmf_ll_min.R2_Shear, bmf_ll_min.M2_Bending));

            Result.Add(string.Format("----------------------------------------------------------------------"));

            bmf_ll.M1_Torsion = bmf_dl.M1_Torsion + bmf_ll_max.M1_Torsion;
            bmf_ll.R2_Shear = bmf_dl.R2_Shear + bmf_ll_max.R2_Shear;
            bmf_ll.M2_Bending = bmf_dl.M2_Bending + bmf_ll_max.M2_Bending;

            Result.Add(string.Format(format, "TOTAL MAXIMUM LOAD", bmf_ll.M1_Torsion, bmf_ll.R2_Shear, bmf_ll.M2_Bending));

            bmf_ll.M1_Torsion = bmf_dl.M1_Torsion + bmf_ll_min.M1_Torsion;
            bmf_ll.R2_Shear = bmf_dl.R2_Shear + bmf_ll_min.R2_Shear;
            bmf_ll.M2_Bending = bmf_dl.M2_Bending + bmf_ll_min.M2_Bending;

            Result.Add(string.Format(format, "TOTAL MINIMUM LOAD", bmf_ll.M1_Torsion, bmf_ll.R2_Shear, bmf_ll.M2_Bending));

            Result.Add(string.Format("----------------------------------------------------------------------"));



            Result.Add(string.Format(""));
            //rtb_ana_result.Lines = Result.ToArray();

            string brg_frc = Path.Combine(user_path, "BEARING_ANALYSIS_RESULT.TXT"); ;

            File.WriteAllLines(brg_frc, Result.ToArray());

            iApp.RunExe(brg_frc);
            uC_BRD1.Read_Reactions(brg_frc);
            Result.Clear();
        }

        void Show_British_Long_Girder_Moment_Shear()
        {
             MemberCollection mc = new MemberCollection(Minor_Analysis.TotalLoad_Analysis.Analysis.Members);

            MemberCollection sort_membs = new MemberCollection();

            JointNodeCollection jn_col = Minor_Analysis.TotalLoad_Analysis.Analysis.Joints;




            double L = Minor_Analysis.Length;
            double W = Minor_Analysis.WidthBridge;
            double val = L / 2;
            int i = 0;

            List<int> _support_inn_joints = new List<int>();
            List<int> _deff_inn_joints = new List<int>();
            List<int> _L8_inn_joints = new List<int>();
            List<int> _L4_inn_joints = new List<int>();
            List<int> _3L8_inn_joints = new List<int>();
            List<int> _L2_inn_joints = new List<int>();



            List<int> _3L16_inn_joints = new List<int>();
            List<int> _5L16_inn_joints = new List<int>();
            List<int> _7L16_inn_joints = new List<int>();



            List<int> _support_out_joints = new List<int>();
            List<int> _deff_out_joints = new List<int>();
            List<int> _L8_out_joints = new List<int>();
            List<int> _L4_out_joints = new List<int>();
            List<int> _3L8_out_joints = new List<int>();
            List<int> _L2_out_joints = new List<int>();


            List<int> _3L16_out_joints = new List<int>();
            List<int> _5L16_out_joints = new List<int>();
            List<int> _7L16_out_joints = new List<int>();



            //List<int> _L4_out_joints = new List<int>();
            //List<int> _deff_out_joints = new List<int>();

            List<int> _cross_joints = new List<int>();


            List<double> _X_joints = new List<double>();
            List<double> _Z_joints = new List<double>();

            for (i = 0; i < jn_col.Count; i++)
            {
                if (_X_joints.Contains(jn_col[i].X) == false) _X_joints.Add(jn_col[i].X);
                if (_Z_joints.Contains(jn_col[i].Z) == false) _Z_joints.Add(jn_col[i].Z);
            }
            //val = MyList.StringToDouble(txt_Ana_eff_depth.Text, -999.0);
            val = Deff;

            List<double> _X_min = new List<double>();
            List<double> _X_max = new List<double>();
            double x_max, x_min;
            double vvv = 99999999999999999;

            //double _L = 0.0;
            for (int zc = 0; zc < _Z_joints.Count; zc++)
            {
                x_min = vvv;
                x_max = -vvv;

                for (i = 0; i < jn_col.Count; i++)
                {
                    //if (_X_joints.Contains(jn_col[i].X) == false) _X_joints.Add(jn_col[i].X);
                    //if (_Z_joints.Contains(jn_col[i].Z) == false) _Z_joints.Add(jn_col[i].Z);

                    if (_Z_joints[zc] == jn_col[i].Z)
                    {
                        if (x_min > jn_col[i].X)
                            x_min = jn_col[i].X;
                        if (x_max < jn_col[i].X)
                            x_max = jn_col[i].X;
                    }

                    //if(jn_col[i].Z == _Z_joints[0])
                    //{
                    //    if (jn_col[i].X > _L)
                    //        _L = jn_col[i].X;
                    //}
                }
                if (x_max != -vvv)
                    _X_max.Add(x_max);
                if (x_min != vvv)
                    _X_min.Add(x_min);

            }

            //val = MyList.StringToDouble(txt_Ana_eff_depth.Text, -999.0);
            val = Minor_Analysis.TotalLoad_Analysis.Analysis.Effective_Depth;


            //L = _L;



            //if (_X_joints.Contains(val))
            //{
            //    Bridge_Analysis.Effective_Depth = val;
            //}
            //else
            //{
            //    Bridge_Analysis.Effective_Depth = _X_joints.Count > 1 ? _X_joints[2] : 0.0; ;
            //}
            //double eff_dep = ;

            //_L_2_joints.Clear();

            double cant_wi_left = Minor_Analysis.Width_LeftCantilever;
            double cant_wi_right = Minor_Analysis.Width_RightCantilever;
            //Bridge_Analysis.Width_LeftCantilever = cant_wi;
            //Bridge_Analysis.Width_RightCantilever = _Z_joints[_Z_joints.Count - 1] - _Z_joints[_Z_joints.Count - 3];


            #region Find Joints
            for (i = 0; i < jn_col.Count; i++)
            {
                try
                {
                    if ((jn_col[i].Z >= cant_wi_left && jn_col[i].Z <= (W - cant_wi_right)) == false) continue;
                    x_min = _X_min[_Z_joints.IndexOf(jn_col[i].Z)];

                    if ((jn_col[i].X.ToString("0.0") == ((L / 2.0) + x_min).ToString("0.0")))
                    {
                        if (jn_col[i].Z >= cant_wi_left && jn_col[i].Z <= (W - cant_wi_right))
                            _L2_inn_joints.Add(jn_col[i].NodeNo);
                    }

                    if (jn_col[i].X.ToString("0.0") == ((3 * L / 8.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _3L8_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (3 * L / 8.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _3L8_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if (jn_col[i].X.ToString("0.0") == ((L / 8.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _L8_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (L / 8.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _L8_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if (jn_col[i].X.ToString("0.0") == ((L / 4.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _L4_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (L / 4.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _L4_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if ((jn_col[i].X.ToString("0.0") == (Minor_Analysis.Effective_Depth + x_min).ToString("0.0")))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _deff_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if ((jn_col[i].X.ToString("0.0") == (L - Minor_Analysis.Effective_Depth + x_min).ToString("0.0")))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _deff_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if (jn_col[i].X.ToString("0.0") == (x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _support_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == (L + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _support_out_joints.Add(jn_col[i].NodeNo);
                    }

                    #region 3L/16


                    if (jn_col[i].X.ToString("0.0") == ((3 * L / 16.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _3L16_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (3 * L / 16.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _3L16_out_joints.Add(jn_col[i].NodeNo);
                    }


                    #endregion 3L/16



                    #region 5L/16


                    if (jn_col[i].X.ToString("0.0") == ((5 * L / 16.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _5L16_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (5 * L / 16.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _5L16_out_joints.Add(jn_col[i].NodeNo);
                    }


                    #endregion 3L/16



                    #region 7L/16


                    if (jn_col[i].X.ToString("0.0") == ((7 * L / 16.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _7L16_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (7 * L / 16.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _7L16_out_joints.Add(jn_col[i].NodeNo);
                    }


                    #endregion 3L/16



                }
                catch (Exception ex) { MessageBox.Show(this, ""); }
            }

            #endregion Find Joints

            Result.Clear();
            Result.Add("");
            Result.Add("");
            //Result.Add("Analysis Result of RCC T-BEAM Bridge");
            Result.Add("");

            #region SHEAR FORCE
            List<double> SF_DL_Self_Weight_G1 = new List<double>();
            List<double> SF_DL_Self_Weight_G2 = new List<double>();

            List<double> SF_DL_Deck_Wet_Conc_G1 = new List<double>();
            List<double> SF_DL_Deck_Wet_Conc_G2 = new List<double>();

            List<double> SF_DL_Deck_Dry_Conc_G1 = new List<double>();
            List<double> SF_DL_Deck_Dry_Conc_G2 = new List<double>();

            List<double> SF_DL_Self_Deck_G1 = new List<double>();
            List<double> SF_DL_Self_Deck_G2 = new List<double>();
            List<double> SF_DL_Self_Deck_G3 = new List<double>();
            List<double> SF_DL_Self_Deck_G4 = new List<double>();

            List<double> SF_SIDL_Crash_Barrier_G1 = new List<double>();
            List<double> SF_SIDL_Crash_Barrier_G2 = new List<double>();
            List<double> SF_SIDL_Crash_Barrier_G3 = new List<double>();
            List<double> SF_SIDL_Crash_Barrier_G4 = new List<double>();

            List<double> SF_SIDL_Wearing_G1 = new List<double>();
            List<double> SF_SIDL_Wearing_G2 = new List<double>();
            List<double> SF_SIDL_Wearing_G3 = new List<double>();
            List<double> SF_SIDL_Wearing_G4 = new List<double>();


            List<double> SF_LL_1 = new List<double>();
            List<double> SF_LL_2 = new List<double>();
            List<double> SF_LL_3 = new List<double>();
            List<double> SF_LL_4 = new List<double>();
            List<double> SF_LL_5 = new List<double>();
            List<double> SF_LL_6 = new List<double>();

            MaxForce f = null;



            for (i = 1; i <= 6; i++)
            {
                #region Dead Load 1
                if (i == 1)
                {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Self_Weight_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Self_Weight_G2.Add(Math.Abs(val));


                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Self_Weight_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Self_Weight_G2.Add(Math.Abs(val));



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G2.Add(Math.Abs(val));

                }
                #endregion Dead Load 1

                #region Dead Load 2
                else if (i == 2)
                {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));


                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));


                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));

                }
                #endregion Dead Load 1

                #region Dead Load 3
                else if (i == 3)
                {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));


                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[1], i);
                    val = f.Force;
                    SF_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));

                }
                #endregion Dead Load 3

                #region Dead Load 4
                else if (i == 4)
                {
                    #region Support
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(Math.Abs(val));

                    if (_support_inn_joints.Count > 2)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_DL_Self_Deck_G3.Add(Math.Abs(val));
                    if (_support_inn_joints.Count > 3)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;
                    SF_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion Support

                    #region Deff
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(Math.Abs(val));

                    if (_deff_inn_joints.Count > 2)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_DL_Self_Deck_G3.Add(Math.Abs(val));

                    if (_deff_inn_joints.Count > 3)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion Support


                    //if (.Count > 3)
                    //{

                    //}
                    //else
                    //    val = 0.0;


                    #region L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(Math.Abs(val));



                    if (_L8_inn_joints.Count > 2)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_DL_Self_Deck_G3.Add(Math.Abs(val));

                    if (_L8_inn_joints.Count > 3)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion  L / 8


                    #region L / 4
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(Math.Abs(val));

                    if (_L4_inn_joints.Count > 2)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_DL_Self_Deck_G3.Add(Math.Abs(val));
                    if (_L4_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[3], i);
                        val = f.Force;

                    }
                    else
                        val = 0.0;

                    SF_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region 3L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(Math.Abs(val));

                    if (_3L8_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[2], i);
                        val = f.Force;

                    }
                    else
                        val = 0.0;

                    SF_DL_Self_Deck_G3.Add(Math.Abs(val));
                    if (_3L8_inn_joints.Count > 3)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 2
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(Math.Abs(val));

                    if (_L2_inn_joints.Count > 2)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_DL_Self_Deck_G3.Add(Math.Abs(val));
                    if (_L2_inn_joints.Count > 3)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion  L / 2


                }
                #endregion Dead Load 4

                #region Dead Load 5
                else if (i == 5)
                {
                    #region Support
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));


                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));
                    if (_support_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[2], i);
                        val = f.Force;

                    }
                    else
                        val = 0.0;
                    SF_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));
                    if (_support_inn_joints.Count > 3)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion Support


                    #region Deff
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));
                    if (_deff_inn_joints.Count > 2)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));
                    if (_deff_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion Deff


                    #region L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));
                    if (_L8_inn_joints.Count > 2)
                    {
                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));

                    if (_L8_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 4
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));

                    if (_L4_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));

                    if (_L4_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region 3L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));

                    if (_3L8_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));

                    if (_3L8_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 2
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));

                    if (_L2_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[2], i);
                        val = f.Force;

                    }
                    else
                        val = 0.0;

                    SF_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));

                    if (_L2_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    SF_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion  L / 2

                }
                #endregion Dead Load 5

                #region Dead Load 6
                else if (i == 6)
                {
                    #region Support
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(Math.Abs(val));

                    if (_support_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Wearing_G3.Add(Math.Abs(val));

                    if (_support_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion Support

                    #region Deff
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(Math.Abs(val));

                    if (_deff_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[2], i);
                        val = f.Force;

                    }
                    else
                        val = 0.0;
                    SF_SIDL_Wearing_G3.Add(Math.Abs(val));

                    if (_deff_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion Deff

                    #region L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(Math.Abs(val));

                    if (_L8_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[2], i);
                        val = f.Force;

                    }
                    else
                        val = 0.0;
                    SF_SIDL_Wearing_G3.Add(Math.Abs(val));

                    if (_L8_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 4
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(Math.Abs(val));

                    if (_L4_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Wearing_G3.Add(Math.Abs(val));

                    if (_L4_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region 3L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(Math.Abs(val));

                    if (_3L8_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Wearing_G3.Add(Math.Abs(val));

                    if (_3L8_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 2
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(Math.Abs(val));

                    if (_L2_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Wearing_G3.Add(Math.Abs(val));

                    if (_L2_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    SF_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion  L / 2

                }
                #endregion Dead Load 5
            }

            List<int> tmp_jts = new List<int>();

            if (rbtn_HA.Checked == false)
            {
                #region Live Load SHEAR Force

                #region Live Load 1

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_ShearForce(_support_inn_joints, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));

                #endregion Support

                #region Live Load 2

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_ShearForce(_support_inn_joints, true);
                val = f.Force;
                SF_LL_2.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_2.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_2.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_2.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_2.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_2.Add(Math.Abs(val));

                #endregion Load 2

                #region Live Load 3

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_ShearForce(_support_inn_joints, true);
                val = f.Force;
                SF_LL_3.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_3.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_3.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_3.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_3.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_3.Add(Math.Abs(val));

                #endregion Load 2

                #region Live Load 4

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_ShearForce(_support_inn_joints, true);
                val = f.Force;
                SF_LL_4.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_4.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_4.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_4.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_4.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_4.Add(Math.Abs(val));

                #endregion Support

                #region Live Load 5

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_ShearForce(_support_inn_joints, true);
                val = f.Force;
                SF_LL_5.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_5.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_5.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_5.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_5.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_5.Add(Math.Abs(val));

                #endregion Support

                #endregion Live Load SHEAR Force
            }
            else
            {

                #region Live Load 1

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_Analysis.GetJoint_ShearForce(_support_inn_joints, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_Analysis.GetJoint_ShearForce(tmp_jts, true);
                val = f.Force;
                SF_LL_1.Add(Math.Abs(val));

                #endregion Support

            }



            MyList.Array_Multiply_With(ref SF_DL_Self_Weight_G1, 10.0);
            MyList.Array_Multiply_With(ref SF_DL_Self_Weight_G2, 10.0);

            MyList.Array_Multiply_With(ref SF_DL_Deck_Wet_Conc_G1, 10.0);
            MyList.Array_Multiply_With(ref SF_DL_Deck_Wet_Conc_G2, 10.0);

            MyList.Array_Multiply_With(ref SF_DL_Deck_Dry_Conc_G1, 10.0);
            MyList.Array_Multiply_With(ref SF_DL_Deck_Dry_Conc_G2, 10.0);

            MyList.Array_Multiply_With(ref SF_DL_Self_Deck_G1, 10.0);
            MyList.Array_Multiply_With(ref SF_DL_Self_Deck_G2, 10.0);
            MyList.Array_Multiply_With(ref SF_DL_Self_Deck_G3, 10.0);
            MyList.Array_Multiply_With(ref SF_DL_Self_Deck_G4, 10.0);

            MyList.Array_Multiply_With(ref SF_SIDL_Crash_Barrier_G1, 10.0);
            MyList.Array_Multiply_With(ref SF_SIDL_Crash_Barrier_G2, 10.0);
            MyList.Array_Multiply_With(ref SF_SIDL_Crash_Barrier_G3, 10.0);
            MyList.Array_Multiply_With(ref SF_SIDL_Crash_Barrier_G4, 10.0);

            MyList.Array_Multiply_With(ref SF_SIDL_Wearing_G1, 10.0);
            MyList.Array_Multiply_With(ref SF_SIDL_Wearing_G2, 10.0);
            MyList.Array_Multiply_With(ref SF_SIDL_Wearing_G3, 10.0);
            MyList.Array_Multiply_With(ref SF_SIDL_Wearing_G4, 10.0);

            MyList.Array_Multiply_With(ref SF_LL_1, 10.0);
            MyList.Array_Multiply_With(ref SF_LL_2, 10.0);
            MyList.Array_Multiply_With(ref SF_LL_3, 10.0);
            MyList.Array_Multiply_With(ref SF_LL_4, 10.0);
            MyList.Array_Multiply_With(ref SF_LL_5, 10.0);
            MyList.Array_Multiply_With(ref SF_LL_6, 10.0);


            #endregion SHEAR FORCE

            #region Bending Moment
            List<double> BM_DL_Self_Weight_G1 = new List<double>();
            List<double> BM_DL_Self_Weight_G2 = new List<double>();

            List<double> BM_DL_Deck_Wet_Conc_G1 = new List<double>();
            List<double> BM_DL_Deck_Wet_Conc_G2 = new List<double>();

            List<double> BM_DL_Deck_Dry_Conc_G1 = new List<double>();
            List<double> BM_DL_Deck_Dry_Conc_G2 = new List<double>();

            List<double> BM_DL_Self_Deck_G1 = new List<double>();
            List<double> BM_DL_Self_Deck_G2 = new List<double>();
            List<double> BM_DL_Self_Deck_G3 = new List<double>();
            List<double> BM_DL_Self_Deck_G4 = new List<double>();

            List<double> BM_SIDL_Crash_Barrier_G1 = new List<double>();
            List<double> BM_SIDL_Crash_Barrier_G2 = new List<double>();
            List<double> BM_SIDL_Crash_Barrier_G3 = new List<double>();
            List<double> BM_SIDL_Crash_Barrier_G4 = new List<double>();

            List<double> BM_SIDL_Wearing_G1 = new List<double>();
            List<double> BM_SIDL_Wearing_G2 = new List<double>();
            List<double> BM_SIDL_Wearing_G3 = new List<double>();
            List<double> BM_SIDL_Wearing_G4 = new List<double>();



            List<double> BM_LL_1 = new List<double>();
            List<double> BM_LL_2 = new List<double>();
            List<double> BM_LL_3 = new List<double>();
            List<double> BM_LL_4 = new List<double>();
            List<double> BM_LL_5 = new List<double>();
            List<double> BM_LL_6 = new List<double>();


            for (i = 1; i <= 6; i++)
            {
                #region Dead Load 1
                if (i == 1)
                {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G2.Add(Math.Abs(val));



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G2.Add(Math.Abs(val));



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[1], i);
                    val = f.Force;
                    BM_DL_Self_Weight_G2.Add(Math.Abs(val));

                }
                #endregion Dead Load 1

                #region Dead Load 2
                else if (i == 2)
                {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[1], i);
                    val = f.Force;
                    BM_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));


                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[1], i);
                    val = f.Force;
                    BM_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[1], i);
                    val = f.Force;
                    BM_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[1], i);
                    val = f.Force;
                    BM_DL_Deck_Wet_Conc_G2.Add(Math.Abs(val));

                }
                #endregion Dead Load 1

                #region Dead Load 3
                else if (i == 3)
                {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[1], i);
                    val = f.Force;
                    BM_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[1], i);
                    val = f.Force;
                    BM_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[1], i);
                    val = f.Force;
                    BM_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[1], i);
                    val = f.Force;
                    BM_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Dry_Conc_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[1], i);
                    val = f.Force;
                    BM_DL_Deck_Dry_Conc_G2.Add(Math.Abs(val));

                }
                #endregion Dead Load 3

                #region Dead Load 4
                else if (i == 4)
                {
                    #region Support
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G2.Add(Math.Abs(val));

                    if (_support_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    BM_DL_Self_Deck_G3.Add(Math.Abs(val));

                    if (_support_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    BM_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion Support

                    #region Deff
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G2.Add(Math.Abs(val));

                    if (_deff_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_DL_Self_Deck_G3.Add(Math.Abs(val));

                    if (_deff_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion Deff

                    #region L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G2.Add(Math.Abs(val));

                    if (_L8_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_DL_Self_Deck_G3.Add(Math.Abs(val));

                    if (_L8_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[3], i);
                        val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 4
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G2.Add(Math.Abs(val));

                    if (_L4_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[2], i);
                        val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_DL_Self_Deck_G3.Add(Math.Abs(val));

                    if (_L4_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region 3L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G2.Add(Math.Abs(val));

                    if (_3L8_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_DL_Self_Deck_G3.Add(Math.Abs(val));

                    if (_3L8_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[3], i);
                        val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 2
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G2.Add(Math.Abs(val));

                    if (_L2_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_DL_Self_Deck_G3.Add(Math.Abs(val));

                    if (_L2_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_DL_Self_Deck_G4.Add(Math.Abs(val));

                    #endregion  L / 8
                }
                #endregion Dead Load 4

                #region Dead Load 5
                else if (i == 5)
                {
                    #region Support
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));

                    if (_support_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));

                    if (_support_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion Support



                    #region Deff
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));

                    if (_deff_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[2], i);
                        val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));

                    if (_deff_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[3], i);
                        val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion Deff


                    #region L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));

                    if (_L8_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));

                    if (_L8_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[3], i);
                        val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 4
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));

                    if (_L4_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[2], i);
                        val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));

                    if (_L4_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region 3L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));

                    if (_3L8_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));

                    if (_3L8_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[3], i);
                        val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 2
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G2.Add(Math.Abs(val));

                    if (_L2_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[2], i);
                        val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_SIDL_Crash_Barrier_G3.Add(Math.Abs(val));

                    if (_L2_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Crash_Barrier_G4.Add(Math.Abs(val));

                    #endregion  L / 2

                }
                #endregion Dead Load 5

                #region Dead Load 6
                else if (i == 6)
                {
                    #region Support
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G2.Add(Math.Abs(val));

                    if (_support_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Wearing_G3.Add(Math.Abs(val));

                    if (_support_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion Support



                    #region Deff
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G2.Add(Math.Abs(val));

                    if (_deff_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[2], i);
                        val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_SIDL_Wearing_G3.Add(Math.Abs(val));

                    if (_deff_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion Deff


                    #region L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G2.Add(Math.Abs(val));

                    if (_L8_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Wearing_G3.Add(Math.Abs(val));

                    if (_L8_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[3], i);
                        val = f.Force;

                    }
                    else
                        val = 0.0;

                    BM_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 4
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G2.Add(Math.Abs(val));

                    if (_L4_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;


                    BM_SIDL_Wearing_G3.Add(Math.Abs(val));

                    if (_L4_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    BM_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region 3L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G2.Add(Math.Abs(val));

                    if (_3L8_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    BM_SIDL_Wearing_G3.Add(Math.Abs(val));

                    if (_3L8_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    BM_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion  L / 8

                    #region L / 2
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G1.Add(Math.Abs(val));

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G2.Add(Math.Abs(val));

                    if (_L2_inn_joints.Count > 2)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[2], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    BM_SIDL_Wearing_G3.Add(Math.Abs(val));

                    if (_L2_inn_joints.Count > 3)
                    {

                        f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[3], i);
                        val = f.Force;
                    }
                    else
                        val = 0.0;

                    BM_SIDL_Wearing_G4.Add(Math.Abs(val));

                    #endregion  L / 2

                }
                #endregion Dead Load 5
            }

            if (rbtn_HA.Checked == false)
            {
                #region Live Load Bending Moments

                #region Live Load 1

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_MomentForce(_support_inn_joints, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));

                #endregion Support

                #region Live Load 2

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_MomentForce(_support_inn_joints, true);
                val = f.Force;
                BM_LL_2.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_2.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_2.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_2.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_2.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_2.Add(Math.Abs(val));

                #endregion Load 2

                #region Live Load 3

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_MomentForce(_support_inn_joints, true);
                val = f.Force;
                BM_LL_3.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_3.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_3.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_3.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_3.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_3.Add(Math.Abs(val));

                #endregion Load 2

                #region Live Load 4

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_MomentForce(_support_inn_joints, true);
                val = f.Force;
                BM_LL_4.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_4.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_4.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_4.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_4.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_4.Add(Math.Abs(val));

                #endregion Support

                #region Live Load 5

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_MomentForce(_support_inn_joints, true);
                val = f.Force;
                BM_LL_5.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_5.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_5.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_5.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_5.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_5.Add(Math.Abs(val));

                #endregion Support


                #region Live Load Bending Moments
                //List<double> 
                #endregion Deck Slab Data


                #endregion Live Load SHEAR Force
            }
            else
            {

                #region Live Load 1

                tmp_jts.Clear();
                tmp_jts.AddRange(_support_inn_joints.ToArray());
                tmp_jts.AddRange(_support_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_Analysis.GetJoint_MomentForce(_support_inn_joints, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_deff_inn_joints.ToArray());
                tmp_jts.AddRange(_deff_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L8_inn_joints.ToArray());
                tmp_jts.AddRange(_L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));

                tmp_jts.Clear();
                tmp_jts.AddRange(_L4_inn_joints.ToArray());
                tmp_jts.AddRange(_L4_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_3L8_inn_joints.ToArray());
                tmp_jts.AddRange(_3L8_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));


                tmp_jts.Clear();
                tmp_jts.AddRange(_L2_inn_joints.ToArray());
                tmp_jts.AddRange(_L2_out_joints.ToArray());
                f = Minor_Analysis.LiveLoad_Analysis.GetJoint_MomentForce(tmp_jts, true);
                val = f.Force;
                BM_LL_1.Add(Math.Abs(val));

                #endregion Support
            }



            MyList.Array_Multiply_With(ref BM_DL_Self_Weight_G1, 10.0);
            MyList.Array_Multiply_With(ref BM_DL_Self_Weight_G2, 10.0);

            MyList.Array_Multiply_With(ref BM_DL_Deck_Wet_Conc_G1, 10.0);
            MyList.Array_Multiply_With(ref BM_DL_Deck_Wet_Conc_G2, 10.0);

            MyList.Array_Multiply_With(ref BM_DL_Deck_Dry_Conc_G1, 10.0);
            MyList.Array_Multiply_With(ref BM_DL_Deck_Dry_Conc_G2, 10.0);

            MyList.Array_Multiply_With(ref BM_DL_Self_Deck_G1, 10.0);
            MyList.Array_Multiply_With(ref BM_DL_Self_Deck_G2, 10.0);
            MyList.Array_Multiply_With(ref BM_DL_Self_Deck_G3, 10.0);
            MyList.Array_Multiply_With(ref BM_DL_Self_Deck_G4, 10.0);

            MyList.Array_Multiply_With(ref BM_SIDL_Crash_Barrier_G1, 10.0);
            MyList.Array_Multiply_With(ref BM_SIDL_Crash_Barrier_G2, 10.0);
            MyList.Array_Multiply_With(ref BM_SIDL_Crash_Barrier_G3, 10.0);
            MyList.Array_Multiply_With(ref BM_SIDL_Crash_Barrier_G4, 10.0);

            MyList.Array_Multiply_With(ref BM_SIDL_Wearing_G1, 10.0);
            MyList.Array_Multiply_With(ref BM_SIDL_Wearing_G2, 10.0);
            MyList.Array_Multiply_With(ref BM_SIDL_Wearing_G3, 10.0);
            MyList.Array_Multiply_With(ref BM_SIDL_Wearing_G4, 10.0);



            MyList.Array_Multiply_With(ref BM_LL_1, 10.0);
            MyList.Array_Multiply_With(ref BM_LL_2, 10.0);
            MyList.Array_Multiply_With(ref BM_LL_3, 10.0);
            MyList.Array_Multiply_With(ref BM_LL_4, 10.0);
            MyList.Array_Multiply_With(ref BM_LL_5, 10.0);
            MyList.Array_Multiply_With(ref BM_LL_6, 10.0);



            #endregion Bending Moment


            #region Cross Girder Data


            double crss_hog, crss_sag, crss_frc;

            _cross_joints.Clear();

            for (i = 0; i < jn_col.Count; i++)
            {
                _cross_joints.Add(jn_col[i].NodeNo);
            }


            List<double> lst_crss_hog = new List<double>();
            List<double> lst_crss_sag = new List<double>();
            List<double> lst_crss_frc = new List<double>();

            crss_hog = Minor_Analysis.TotalLoad_Analysis.GetJoint_Max_Hogging(_cross_joints, true);
            crss_sag = Minor_Analysis.TotalLoad_Analysis.GetJoint_Max_Sagging(_cross_joints, true);
            crss_frc = Minor_Analysis.TotalLoad_Analysis.GetJoint_ShearForce(_cross_joints);

            lst_crss_hog.Add(crss_hog);
            lst_crss_sag.Add(crss_sag);
            lst_crss_frc.Add(crss_frc);

            if (rbtn_HA.Checked == false)
            {
                crss_hog = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Hogging(_cross_joints, true);
                crss_sag = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Sagging(_cross_joints, true);
                crss_frc = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_ShearForce(_cross_joints);

                lst_crss_hog.Add(crss_hog);
                lst_crss_sag.Add(crss_sag);
                lst_crss_frc.Add(crss_frc);


                crss_hog = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Hogging(_cross_joints, true);
                crss_sag = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Sagging(_cross_joints, true);
                crss_frc = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_ShearForce(_cross_joints);

                lst_crss_hog.Add(crss_hog);
                lst_crss_sag.Add(crss_sag);
                lst_crss_frc.Add(crss_frc);


                crss_hog = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Hogging(_cross_joints, true);
                crss_sag = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Sagging(_cross_joints, true);
                crss_frc = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_ShearForce(_cross_joints);

                lst_crss_hog.Add(crss_hog);
                lst_crss_sag.Add(crss_sag);
                lst_crss_frc.Add(crss_frc);


                crss_hog = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Hogging(_cross_joints, true);
                crss_sag = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Sagging(_cross_joints, true);
                crss_frc = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_ShearForce(_cross_joints);

                lst_crss_hog.Add(crss_hog);
                lst_crss_sag.Add(crss_sag);
                lst_crss_frc.Add(crss_frc);


                crss_hog = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Hogging(_cross_joints, true);
                crss_sag = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Sagging(_cross_joints, true);
                crss_frc = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_ShearForce(_cross_joints);

                lst_crss_hog.Add(crss_hog);
                lst_crss_sag.Add(crss_sag);
                lst_crss_frc.Add(crss_frc);
            }

            lst_crss_hog.Sort();
            lst_crss_hog.Reverse();
            lst_crss_sag.Sort();
            //lst_crss_sag.Reverse();
            lst_crss_frc.Sort();
            lst_crss_frc.Reverse();

            if (dgv_cross_user_input.RowCount > 7)
            {
                dgv_cross_user_input[1, dgv_cross_user_input.RowCount - 1].Value = Math.Abs(lst_crss_frc[0]).ToString("f3");
                dgv_cross_user_input[1, dgv_cross_user_input.RowCount - 2].Value = Math.Abs(lst_crss_sag[0]).ToString("f3");
                dgv_cross_user_input[1, dgv_cross_user_input.RowCount - 3].Value = Math.Abs(lst_crss_hog[0]).ToString("f3");
            }
            #endregion Cross Girder Data

            val = 0.0;


            string format = "{0,-30} {1,12} {2,10:f3} {3,15:f3} {4,10:f3} {5,10:f3} {6,10:f3} {7,10:f3}";

            Result.Add(string.Format(""));

            #region Summary 1

            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------"));
            Result.Add(string.Format("RCC T GIRDER (LIMIT STATE METHOD) ANALYSIS RESULTS "));
            Result.Add(string.Format("---------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of BM and SF for different load cases ( Unfactored)"));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of B.M. & S.F. due to Dead Load (Forces due to Self weight of girder) kN-m"));
            Result.Add(string.Format("------------------------------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("     Girder No                  Components    Support       Web widening    1/8th     1/4th      3/8th        Mid "));
            Result.Add(string.Format("                                                                 end        span      span       span        span"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));


            Result.Add(string.Format(format, "G1 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Self_Weight_G1[0],
                                                BM_DL_Self_Weight_G1[1],
                                                BM_DL_Self_Weight_G1[2],
                                                BM_DL_Self_Weight_G1[3],
                                                BM_DL_Self_Weight_G1[4],
                                                BM_DL_Self_Weight_G1[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN", SF_DL_Self_Weight_G1[0],
                                                SF_DL_Self_Weight_G1[1],
                                                SF_DL_Self_Weight_G1[2],
                                                SF_DL_Self_Weight_G1[3],
                                                SF_DL_Self_Weight_G1[4],
                                                SF_DL_Self_Weight_G1[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G2 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Self_Weight_G2[0],
                                                BM_DL_Self_Weight_G2[1],
                                                BM_DL_Self_Weight_G2[2],
                                                BM_DL_Self_Weight_G2[3],
                                                BM_DL_Self_Weight_G2[4],
                                                BM_DL_Self_Weight_G2[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Self_Weight_G2[0],
                                                SF_DL_Self_Weight_G2[1],
                                                SF_DL_Self_Weight_G2[2],
                                                SF_DL_Self_Weight_G2[3],
                                                SF_DL_Self_Weight_G2[4],
                                                SF_DL_Self_Weight_G2[5]));

            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            #endregion Summary 1

            #region Summary 2

            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of B.M. & S.F. due to Dead Load (Forces due to Deck slab Wet concrete and Shuttering load)"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("     Girder No                  Components    Support       Web widening    1/8th     1/4th      3/8th        Mid "));
            Result.Add(string.Format("                                                                 end        span      span       span        span"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(format, "G1 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Deck_Wet_Conc_G1[0],
                                                BM_DL_Deck_Wet_Conc_G1[1],
                                                BM_DL_Deck_Wet_Conc_G1[2],
                                                BM_DL_Deck_Wet_Conc_G1[3],
                                                BM_DL_Deck_Wet_Conc_G1[4],
                                                BM_DL_Deck_Wet_Conc_G1[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Deck_Wet_Conc_G1[0],
                                                SF_DL_Deck_Wet_Conc_G1[1],
                                                SF_DL_Deck_Wet_Conc_G1[2],
                                                SF_DL_Deck_Wet_Conc_G1[3],
                                                SF_DL_Deck_Wet_Conc_G1[4],
                                                SF_DL_Deck_Wet_Conc_G1[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G2 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Deck_Wet_Conc_G2[0],
                                                BM_DL_Deck_Wet_Conc_G2[1],
                                                BM_DL_Deck_Wet_Conc_G2[2],
                                                BM_DL_Deck_Wet_Conc_G2[3],
                                                BM_DL_Deck_Wet_Conc_G2[4],
                                                BM_DL_Deck_Wet_Conc_G2[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Deck_Wet_Conc_G2[0],
                                                SF_DL_Deck_Wet_Conc_G2[1],
                                                SF_DL_Deck_Wet_Conc_G2[2],
                                                SF_DL_Deck_Wet_Conc_G2[3],
                                                SF_DL_Deck_Wet_Conc_G2[4],
                                                SF_DL_Deck_Wet_Conc_G2[5]));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            #endregion Summary 2

            #region Summary 3

            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of B.M. & S.F. due to Deshutering load"));
            Result.Add(string.Format("-----------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("     Girder No                  Components    Support       Web widening    1/8th     1/4th      3/8th        Mid "));
            Result.Add(string.Format("                                                                 end        span      span       span        span"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(format, "G1 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Deck_Dry_Conc_G1[0],
                                                BM_DL_Deck_Dry_Conc_G1[1],
                                                BM_DL_Deck_Dry_Conc_G1[2],
                                                BM_DL_Deck_Dry_Conc_G1[3],
                                                BM_DL_Deck_Dry_Conc_G1[4],
                                                BM_DL_Deck_Dry_Conc_G1[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Deck_Dry_Conc_G1[0],
                                                SF_DL_Deck_Dry_Conc_G1[1],
                                                SF_DL_Deck_Dry_Conc_G1[2],
                                                SF_DL_Deck_Dry_Conc_G1[3],
                                                SF_DL_Deck_Dry_Conc_G1[4],
                                                SF_DL_Deck_Dry_Conc_G1[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G2 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Deck_Dry_Conc_G2[0],
                                                BM_DL_Deck_Dry_Conc_G2[1],
                                                BM_DL_Deck_Dry_Conc_G2[2],
                                                BM_DL_Deck_Dry_Conc_G2[3],
                                                BM_DL_Deck_Dry_Conc_G2[4],
                                                BM_DL_Deck_Dry_Conc_G2[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Deck_Dry_Conc_G2[0],
                                                SF_DL_Deck_Dry_Conc_G2[1],
                                                SF_DL_Deck_Dry_Conc_G2[2],
                                                SF_DL_Deck_Dry_Conc_G2[3],
                                                SF_DL_Deck_Dry_Conc_G2[4],
                                                SF_DL_Deck_Dry_Conc_G2[5]));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            #endregion Summary 3

            #region Summary 4

            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of B.M. & S.F. per girder due to Dead Load (Forces due to Self weight of girder,Deck slab dry concrete)"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("     Girder No                  Components    Support       Web widening    1/8th     1/4th      3/8th        Mid "));
            Result.Add(string.Format("                                                                 end        span      span       span        span"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(format, "G1 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Self_Deck_G1[0],
                                                BM_DL_Self_Deck_G1[1],
                                                BM_DL_Self_Deck_G1[2],
                                                BM_DL_Self_Deck_G1[3],
                                                BM_DL_Self_Deck_G1[4],
                                                BM_DL_Self_Deck_G1[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Self_Deck_G1[0],
                                                SF_DL_Self_Deck_G1[1],
                                                SF_DL_Self_Deck_G1[2],
                                                SF_DL_Self_Deck_G1[3],
                                                SF_DL_Self_Deck_G1[4],
                                                SF_DL_Self_Deck_G1[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G2 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Self_Deck_G2[0],
                                                BM_DL_Self_Deck_G2[1],
                                                BM_DL_Self_Deck_G2[2],
                                                BM_DL_Self_Deck_G2[3],
                                                BM_DL_Self_Deck_G2[4],
                                                BM_DL_Self_Deck_G2[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Self_Deck_G2[0],
                                                SF_DL_Self_Deck_G2[1],
                                                SF_DL_Self_Deck_G2[2],
                                                SF_DL_Self_Deck_G2[3],
                                                SF_DL_Self_Deck_G2[4],
                                                SF_DL_Self_Deck_G2[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G3 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Self_Deck_G3[0],
                                                BM_DL_Self_Deck_G3[1],
                                                BM_DL_Self_Deck_G3[2],
                                                BM_DL_Self_Deck_G3[3],
                                                BM_DL_Self_Deck_G3[4],
                                                BM_DL_Self_Deck_G3[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Self_Deck_G3[0],
                                                SF_DL_Self_Deck_G3[1],
                                                SF_DL_Self_Deck_G3[2],
                                                SF_DL_Self_Deck_G3[3],
                                                SF_DL_Self_Deck_G3[4],
                                                SF_DL_Self_Deck_G3[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G4 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Self_Deck_G4[0],
                                                BM_DL_Self_Deck_G4[1],
                                                BM_DL_Self_Deck_G4[2],
                                                BM_DL_Self_Deck_G4[3],
                                                BM_DL_Self_Deck_G4[4],
                                                BM_DL_Self_Deck_G4[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Self_Deck_G4[0],
                                                SF_DL_Self_Deck_G4[1],
                                                SF_DL_Self_Deck_G4[2],
                                                SF_DL_Self_Deck_G4[3],
                                                SF_DL_Self_Deck_G4[4],
                                                SF_DL_Self_Deck_G4[5]));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            #endregion Summary 4

            #region Summary 5

            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of B.M. & S.F. per girder due to SIDL(Crash barrier)"));
            Result.Add(string.Format("------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("     Girder No                  Components    Support       Web widening    1/8th     1/4th      3/8th        Mid "));
            Result.Add(string.Format("                                                                 end        span      span       span        span"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(format, "G1 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Crash_Barrier_G1[0],
                                                BM_SIDL_Crash_Barrier_G1[1],
                                                BM_SIDL_Crash_Barrier_G1[2],
                                                BM_SIDL_Crash_Barrier_G1[3],
                                                BM_SIDL_Crash_Barrier_G1[4],
                                                BM_SIDL_Crash_Barrier_G1[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Crash_Barrier_G1[0],
                                                SF_SIDL_Crash_Barrier_G1[1],
                                                SF_SIDL_Crash_Barrier_G1[2],
                                                SF_SIDL_Crash_Barrier_G1[3],
                                                SF_SIDL_Crash_Barrier_G1[4],
                                                SF_SIDL_Crash_Barrier_G1[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G2 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Crash_Barrier_G2[0],
                                                BM_SIDL_Crash_Barrier_G2[1],
                                                BM_SIDL_Crash_Barrier_G2[2],
                                                BM_SIDL_Crash_Barrier_G2[3],
                                                BM_SIDL_Crash_Barrier_G2[4],
                                                BM_SIDL_Crash_Barrier_G2[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Crash_Barrier_G2[0],
                                                SF_SIDL_Crash_Barrier_G2[1],
                                                SF_SIDL_Crash_Barrier_G2[2],
                                                SF_SIDL_Crash_Barrier_G2[3],
                                                SF_SIDL_Crash_Barrier_G2[4],
                                                SF_SIDL_Crash_Barrier_G2[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G3 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Crash_Barrier_G3[0],
                                                BM_SIDL_Crash_Barrier_G3[1],
                                                BM_SIDL_Crash_Barrier_G3[2],
                                                BM_SIDL_Crash_Barrier_G3[3],
                                                BM_SIDL_Crash_Barrier_G3[4],
                                                BM_SIDL_Crash_Barrier_G3[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Crash_Barrier_G3[0],
                                                SF_SIDL_Crash_Barrier_G3[1],
                                                SF_SIDL_Crash_Barrier_G3[2],
                                                SF_SIDL_Crash_Barrier_G3[3],
                                                SF_SIDL_Crash_Barrier_G3[4],
                                                SF_SIDL_Crash_Barrier_G3[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G4 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Crash_Barrier_G4[0],
                                                BM_SIDL_Crash_Barrier_G4[1],
                                                BM_SIDL_Crash_Barrier_G4[2],
                                                BM_SIDL_Crash_Barrier_G4[3],
                                                BM_SIDL_Crash_Barrier_G4[4],
                                                BM_SIDL_Crash_Barrier_G4[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Crash_Barrier_G4[0],
                                                SF_SIDL_Crash_Barrier_G4[1],
                                                SF_SIDL_Crash_Barrier_G4[2],
                                                SF_SIDL_Crash_Barrier_G4[3],
                                                SF_SIDL_Crash_Barrier_G4[4],
                                                SF_SIDL_Crash_Barrier_G4[5]));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            #endregion Summary 5

            #region Summary 6

            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of B.M. & S.F. per girder due to SIDL(Wearing coat)"));
            Result.Add(string.Format("------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("     Girder No                  Components    Support       Web widening    1/8th     1/4th      3/8th        Mid "));
            Result.Add(string.Format("                                                                 end        span      span       span        span"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(format, "G1 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Wearing_G1[0],
                                                BM_SIDL_Wearing_G1[1],
                                                BM_SIDL_Wearing_G1[2],
                                                BM_SIDL_Wearing_G1[3],
                                                BM_SIDL_Wearing_G1[4],
                                                BM_SIDL_Wearing_G1[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Wearing_G1[0],
                                                SF_SIDL_Wearing_G1[1],
                                                SF_SIDL_Wearing_G1[2],
                                                SF_SIDL_Wearing_G1[3],
                                                SF_SIDL_Wearing_G1[4],
                                                SF_SIDL_Wearing_G1[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G2 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Wearing_G2[0],
                                                BM_SIDL_Wearing_G2[1],
                                                BM_SIDL_Wearing_G2[2],
                                                BM_SIDL_Wearing_G2[3],
                                                BM_SIDL_Wearing_G2[4],
                                                BM_SIDL_Wearing_G2[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Wearing_G2[0],
                                                SF_SIDL_Wearing_G2[1],
                                                SF_SIDL_Wearing_G2[2],
                                                SF_SIDL_Wearing_G2[3],
                                                SF_SIDL_Wearing_G2[4],
                                                SF_SIDL_Wearing_G2[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G3 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Wearing_G3[0],
                                                BM_SIDL_Wearing_G3[1],
                                                BM_SIDL_Wearing_G3[2],
                                                BM_SIDL_Wearing_G3[3],
                                                BM_SIDL_Wearing_G3[4],
                                                BM_SIDL_Wearing_G3[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Wearing_G3[0],
                                                SF_SIDL_Wearing_G3[1],
                                                SF_SIDL_Wearing_G3[2],
                                                SF_SIDL_Wearing_G3[3],
                                                SF_SIDL_Wearing_G3[4],
                                                SF_SIDL_Wearing_G3[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G4 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Wearing_G4[0],
                                                BM_SIDL_Wearing_G4[1],
                                                BM_SIDL_Wearing_G4[2],
                                                BM_SIDL_Wearing_G4[3],
                                                BM_SIDL_Wearing_G4[4],
                                                BM_SIDL_Wearing_G4[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Wearing_G4[0],
                                                SF_SIDL_Wearing_G4[1],
                                                SF_SIDL_Wearing_G4[2],
                                                SF_SIDL_Wearing_G4[3],
                                                SF_SIDL_Wearing_G4[4],
                                                SF_SIDL_Wearing_G4[5]));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            #endregion Summary 6

            #region Summary 7

            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of B.M. & S.F. per girder due to Live Load"));
            Result.Add(string.Format("---------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("    Loading Analysis            Components    Support       Web widening    1/8th     1/4th      3/8th        Mid "));
            Result.Add(string.Format("                                                                 end        span      span       span        span"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            string ld_text = "";

            if (rbtn_HA.Checked)
                ld_text = "HA Loading ";

            if (rbtn_HA.Checked)
            {

                Result.Add(string.Format(format, "LL1 ( " + ld_text + " )",
                                                    "BM in kN-m",
                                                    BM_LL_1[0],
                                                    BM_LL_1[1],
                                                    BM_LL_1[2],
                                                    BM_LL_1[3],
                                                    BM_LL_1[4],
                                                    BM_LL_1[5]));
                //Result.Add(string.Format(""));
                Result.Add(string.Format(format, "", "SF in kN",
                                                    SF_LL_1[0],
                                                    SF_LL_1[1],
                                                    SF_LL_1[2],
                                                    SF_LL_1[3],
                                                    SF_LL_1[4],
                                                    SF_LL_1[5]));
                Result.Add(string.Format(""));
            }
            else if (rbtn_HA_HB.Checked || rbtn_HB.Checked)
            {
                if (rbtn_HA_HB.Checked)
                    ld_text = "HA & " + cmb_HB.SelectedItem;
                else
                    ld_text = cmb_HB.SelectedItem.ToString();
                
                Result.Add(string.Format(format, "LL1 ( " + ld_text + "_6 )",
                                                    "BM in kN-m",
                                                    BM_LL_1[0],
                                                    BM_LL_1[1],
                                                    BM_LL_1[2],
                                                    BM_LL_1[3],
                                                    BM_LL_1[4],
                                                    BM_LL_1[5]));
                //Result.Add(string.Format(""));
                Result.Add(string.Format(format, "", "SF in kN",
                                                    SF_LL_1[0],
                                                    SF_LL_1[1],
                                                    SF_LL_1[2],
                                                    SF_LL_1[3],
                                                    SF_LL_1[4],
                                                    SF_LL_1[5]));
                Result.Add(string.Format(""));
                Result.Add(string.Format(format, "LL2( " + ld_text + "_11 )",

                    "BM in kN-m",
                                                   BM_LL_2[0],
                                                   BM_LL_2[1],
                                                   BM_LL_2[2],
                                                   BM_LL_2[3],
                                                   BM_LL_2[4],
                                                   BM_LL_2[5]));
                //Result.Add(string.Format(""));
                Result.Add(string.Format(format, "", "SF in kN",
                                                    SF_LL_2[0],
                                                    SF_LL_2[1],
                                                    SF_LL_2[2],
                                                    SF_LL_2[3],
                                                    SF_LL_2[4],
                                                    SF_LL_2[5]));
                Result.Add(string.Format(""));
                Result.Add(string.Format(format, "LL3( " + ld_text + "_16 )", "BM in kN-m",
                                                   BM_LL_3[0],
                                                   BM_LL_3[1],
                                                   BM_LL_3[2],
                                                   BM_LL_3[3],
                                                   BM_LL_3[4],
                                                   BM_LL_3[5]));
                //Result.Add(string.Format(""));
                Result.Add(string.Format(format, "", "SF in kN",
                                                    SF_LL_3[0],
                                                    SF_LL_3[1],
                                                    SF_LL_3[2],
                                                    SF_LL_3[3],
                                                    SF_LL_3[4],
                                                    SF_LL_3[5]));
                Result.Add(string.Format(""));
                Result.Add(string.Format(format, "LL4( " + ld_text + "_21 )", "BM in kN-m",
                                                   BM_LL_4[0],
                                                   BM_LL_4[1],
                                                   BM_LL_4[2],
                                                   BM_LL_4[3],
                                                   BM_LL_4[4],
                                                   BM_LL_4[5]));
                //Result.Add(string.Format(""));
                Result.Add(string.Format(format, "", "SF in kN",
                                                    SF_LL_4[0],
                                                    SF_LL_4[1],
                                                    SF_LL_4[2],
                                                    SF_LL_4[3],
                                                    SF_LL_4[4],
                                                    SF_LL_4[5]));
                Result.Add(string.Format(""));
                Result.Add(string.Format(format, "LL5( " + ld_text + "_26 )", "BM in kN-m",
                                                   BM_LL_5[0],
                                                   BM_LL_5[1],
                                                   BM_LL_5[2],
                                                   BM_LL_5[3],
                                                   BM_LL_5[4],
                                                   BM_LL_5[5]));
                //Result.Add(string.Format(""));
                Result.Add(string.Format(format, "", "SF in kN",
                                                    SF_LL_5[0],
                                                    SF_LL_5[1],
                                                    SF_LL_5[2],
                                                    SF_LL_5[3],
                                                    SF_LL_5[4],
                                                    SF_LL_5[5]));
            }

            //Result.Add(string.Format(""));
            //Result.Add(string.Format(format, "LL6(1L TYPE 3 + 1L TYPE 1)", "BM in kN-m",
            //                                   BM_LL_6[0],
            //                                   BM_LL_6[1],
            //                                   BM_LL_6[2],
            //                                   BM_LL_6[3],
            //                                   BM_LL_6[4],
            //                                   BM_LL_6[5]));
            ////Result.Add(string.Format(""));
            //Result.Add(string.Format(format, "", "SF in kN",
            //                                    SF_LL_6[0],
            //                                    SF_LL_6[1],
            //                                    SF_LL_6[2],
            //                                    SF_LL_6[3],
            //                                    SF_LL_6[4],
            //                                    SF_LL_6[5]));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            #endregion Summary 7



            List<int> outer_joints = new List<int>();
            List<int> outer_joints_right = new List<int>();
            if (_support_inn_joints.Count > 0)
            {
                outer_joints.Add(_support_inn_joints[0]);
                outer_joints_right.Add(_support_inn_joints[_support_inn_joints.Count - 1]);
            }


            if (_L8_inn_joints.Count > 0)
                outer_joints.Add(_L8_inn_joints[0]);
            outer_joints_right.Add(_L8_inn_joints[_L8_inn_joints.Count - 1]);

            if (_3L16_inn_joints.Count > 0)
            {
                outer_joints.Add(_3L16_inn_joints[0]);
                outer_joints_right.Add(_3L16_inn_joints[_3L16_inn_joints.Count - 1]);
            }

            if (_L4_inn_joints.Count > 0)
            {
                outer_joints.Add(_L4_inn_joints[0]);
                outer_joints_right.Add(_L4_inn_joints[_L4_inn_joints.Count - 1]);
            }
            if (_5L16_inn_joints.Count > 0)
            {
                outer_joints.Add(_5L16_inn_joints[0]);
                outer_joints_right.Add(_5L16_inn_joints[_5L16_inn_joints.Count - 1]);
            }
            if (_3L8_inn_joints.Count > 0)
            {
                outer_joints.Add(_3L8_inn_joints[0]);
                outer_joints_right.Add(_3L8_inn_joints[_3L8_inn_joints.Count - 1]);
            }
            if (_7L16_inn_joints.Count > 0)
            {
                outer_joints.Add(_7L16_inn_joints[0]);
                outer_joints_right.Add(_7L16_inn_joints[_7L16_inn_joints.Count - 1]);
            }
            if (_L2_inn_joints.Count > 0)
            {
                outer_joints.Add(_L2_inn_joints[0]);
                outer_joints_right.Add(_L2_inn_joints[_L2_inn_joints.Count - 1]);
            }


            if (_7L16_out_joints.Count > 0)
            {
                outer_joints.Add(_7L16_out_joints[0]);
                outer_joints_right.Add(_7L16_out_joints[_7L16_out_joints.Count - 1]);
            }



            if (_3L8_out_joints.Count > 0)
            {
                outer_joints.Add(_3L8_out_joints[0]);
                outer_joints_right.Add(_3L8_out_joints[_3L8_out_joints.Count - 1]);
            }



            if (_5L16_out_joints.Count > 0)
            {
                outer_joints.Add(_5L16_out_joints[0]);
                outer_joints_right.Add(_5L16_out_joints[_5L16_out_joints.Count - 1]);
            }


            if (_L4_out_joints.Count > 0)
            {
                outer_joints.Add(_L4_out_joints[0]);
                outer_joints_right.Add(_L4_out_joints[_L4_out_joints.Count - 1]);
            }


            if (_3L16_out_joints.Count > 0)
            {
                outer_joints.Add(_3L16_out_joints[0]);
                outer_joints_right.Add(_3L16_out_joints[_3L16_out_joints.Count - 1]);
            }


            if (_L8_out_joints.Count > 0)
            {
                outer_joints.Add(_L8_out_joints[0]);
                outer_joints_right.Add(_L8_out_joints[_L8_out_joints.Count - 1]);
            }


            if (_support_out_joints.Count > 0)
            {
                outer_joints.Add(_support_out_joints[0]);
                outer_joints_right.Add(_support_out_joints[_support_out_joints.Count - 1]);
            }





            //iApp.Progress_ON("Reading Maximum deflection....");


            List<NodeResultData> lst_nrd = new List<NodeResultData>();


            lst_nrd.Add(Minor_Analysis.TotalLoad_Analysis.Node_Displacements.Get_Max_Deflection());
            if (rbtn_HA.Checked)
            {
                lst_nrd.Add(Minor_Analysis.LiveLoad_Analysis.Node_Displacements.Get_Max_Deflection());
            }
            else
            {
                lst_nrd.Add(Minor_Analysis.LiveLoad_1_Analysis.Node_Displacements.Get_Max_Deflection());
                lst_nrd.Add(Minor_Analysis.LiveLoad_2_Analysis.Node_Displacements.Get_Max_Deflection());
                lst_nrd.Add(Minor_Analysis.LiveLoad_3_Analysis.Node_Displacements.Get_Max_Deflection());
                lst_nrd.Add(Minor_Analysis.LiveLoad_4_Analysis.Node_Displacements.Get_Max_Deflection());
                lst_nrd.Add(Minor_Analysis.LiveLoad_5_Analysis.Node_Displacements.Get_Max_Deflection());
            }
            //lst_nrd.Add(Long_Girder_Analysis.LiveLoad_6_Analysis.Node_Displacements.Get_Max_Deflection());

            //lst_nrd.Add(Long_Girder_Analysis.DeadLoad_Analysis.Node_Displacements.Get_Max_Deflection());


            int max_indx = 0;
            double max_def, allow_def;


            max_def = 0;
            allow_def = (L / 800.0);

            for (i = 0; i < lst_nrd.Count; i++)
            {
                if (max_def < Math.Abs(lst_nrd[i].Max_Translation))
                {
                    max_def = Math.Abs(lst_nrd[i].Max_Translation);
                    max_indx = i;
                }
            }



            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------"));
            Result.Add(string.Format("CHECK FOR LIVE LOAD DEFLECTION"));
            Result.Add(string.Format("---------------------------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(" MAXIMUM     NODE DISPLACEMENTS / ROTATIONS"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(" NODE     LOAD          X-            Y-            Z-             X-               Y-            Z-"));
            Result.Add(string.Format(" NUMBER   CASE    TRANSLATION    TRANSLATION    TRANSLATION     ROTATION        ROTATION      ROTATION"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(lst_nrd[max_indx].ToString()));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));


            Result.Add(string.Format("ALLOWABLE DEFLECTION = SPAN/800 M. = {0}/800 M. = {1} M. ", L, allow_def));
            Result.Add(string.Format(""));
            if (max_def > allow_def)
                Result.Add(string.Format("MAXIMUM  VERTICAL DEFLECTION = {0:f6} M. > {1:f6} M. ", max_def, allow_def));
            else
                Result.Add(string.Format("MAXIMUM  VERTICAL DEFLECTION = {0:f6} M. < {1:f6} M. ", max_def, allow_def));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------"));
            Result.Add(string.Format("CHECK FOR DEAD LOAD DEFLECTION LEFT SIDE OUTER GIRDER"));
            Result.Add(string.Format("---------------------------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(" MAXIMUM     NODE DISPLACEMENTS / ROTATIONS"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(" NODE     LOAD          X-            Y-            Z-             X-               Y-            Z-"));
            Result.Add(string.Format(" NUMBER   CASE    TRANSLATION    TRANSLATION    TRANSLATION     ROTATION        ROTATION      ROTATION"));
            Result.Add(string.Format(""));
            lst_nrd.Clear();
            for (i = 0; i < outer_joints.Count; i++)
            {
                lst_nrd.Add(Minor_Analysis.DeadLoad_Analysis.Node_Displacements.Get_Node_Deflection(outer_joints[i]));
                Result.Add(string.Format(lst_nrd[i].ToString()));
            }
            //iApp.SetProgressValue(70, 100);

            Result.Add(string.Format(""));
            Result.Add(string.Format("ALLOWABLE DEFLECTION = SPAN/800 M. = {0}/800 M. = {1} M. ", L, allow_def));
            Result.Add(string.Format(""));
            Result.Add(string.Format("MAXIMUM NODE DISPLACEMENTS FOR LEFT SIDE OUTER LONG GIRDER"));
            Result.Add(string.Format("----------------------------------------------------------"));
            Result.Add(string.Format(""));
            for (i = 0; i < lst_nrd.Count; i++)
            {
                if (lst_nrd[i].Max_Translation < allow_def)
                    Result.Add(string.Format("MAXIMUM  VERTICAL DISPLACEMENT AT NODE {0}  = {1:f6} M. < {2:f6} M.    OK.", lst_nrd[i].NodeNo, lst_nrd[i].Max_Translation, allow_def));
                else
                    Result.Add(string.Format("MAXIMUM  VERTICAL DISPLACEMENT AT NODE {0}  = {1:f6} M. > {2:f6} M.    NOT OK.", lst_nrd[i].NodeNo, Math.Abs(lst_nrd[i].Max_Translation), allow_def));
            }

            //iApp.SetProgressValue(80, 100);


            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------"));
            Result.Add(string.Format("CHECK FOR DEAD LOAD DEFLECTION RIGHT SIDE OUTER GIRDER"));
            Result.Add(string.Format("---------------------------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(" MAXIMUM     NODE DISPLACEMENTS / ROTATIONS"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(" NODE     LOAD          X-            Y-            Z-             X-               Y-            Z-"));
            Result.Add(string.Format(" NUMBER   CASE    TRANSLATION    TRANSLATION    TRANSLATION     ROTATION        ROTATION      ROTATION"));
            Result.Add(string.Format(""));
            lst_nrd.Clear();
            for (i = 0; i < outer_joints_right.Count; i++)
            {
                lst_nrd.Add(Minor_Analysis.DeadLoad_Analysis.Node_Displacements.Get_Node_Deflection(outer_joints_right[i]));
                Result.Add(string.Format(lst_nrd[i].ToString()));
            }
            //iApp.SetProgressValue(90, 100);

            Result.Add(string.Format(""));
            Result.Add(string.Format("ALLOWABLE DEFLECTION = SPAN/800 M. = {0}/800 M. = {1} M. ", L, allow_def));
            Result.Add(string.Format(""));
            Result.Add(string.Format("MAXIMUM NODE DISPLACEMENTS FOR RIGHT SIDE OUTER LONG GIRDER"));
            Result.Add(string.Format("------------------------------------------------------------"));
            Result.Add(string.Format(""));
            for (i = 0; i < lst_nrd.Count; i++)
            {
                if (lst_nrd[i].Max_Translation < allow_def)
                    Result.Add(string.Format("MAXIMUM  VERTICAL DISPLACEMENT AT NODE {0}  = {1:f6} M. < {2:f6} M.    OK.", lst_nrd[i].NodeNo, lst_nrd[i].Max_Translation, allow_def));
                else
                    Result.Add(string.Format("MAXIMUM  VERTICAL DISPLACEMENT AT NODE {0}  = {1:f6} M. > {2:f6} M.    NOT OK.", lst_nrd[i].NodeNo, Math.Abs(lst_nrd[i].Max_Translation), allow_def));
            }


            //iApp.SetProgressValue(100, 100);



            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("The Deflection for Dead Load is to be controlled by providing Longitudinal"));
            Result.Add(string.Format("Camber along the length of the Main Girder between end to end supports."));






            //iApp.Progress_OFF();




            Result.Add(string.Format(""));
            rtb_ana_result.Lines = Result.ToArray();

            File.WriteAllLines(File_Long_Girder_Results, Result.ToArray());






            //Long_Girder_Analysis.DeadLoad_Analysis.Node_Displacements

            iApp.RunExe(File_Long_Girder_Results);
            Result.Add(string.Format(""));
        }

        void Show_Long_Girder_Moment_Shear_2014_03_10()
        {
            MemberCollection mc = new MemberCollection(Minor_Analysis.TotalLoad_Analysis.Analysis.Members);

            MemberCollection sort_membs = new MemberCollection();

            JointNodeCollection jn_col = Minor_Analysis.TotalLoad_Analysis.Analysis.Joints;




            double L = Minor_Analysis.Length;
            double W = Minor_Analysis.WidthBridge;
            double val = L / 2;
            int i = 0;

            List<int> _support_inn_joints = new List<int>();
            List<int> _deff_inn_joints = new List<int>();
            List<int> _L8_inn_joints = new List<int>();
            List<int> _L4_inn_joints = new List<int>();
            List<int> _3L8_inn_joints = new List<int>();
            List<int> _L2_inn_joints = new List<int>();



            List<int> _3L16_inn_joints = new List<int>();
            List<int> _5L16_inn_joints = new List<int>();
            List<int> _7L16_inn_joints = new List<int>();



            List<int> _support_out_joints = new List<int>();
            List<int> _deff_out_joints = new List<int>();
            List<int> _L8_out_joints = new List<int>();
            List<int> _L4_out_joints = new List<int>();
            List<int> _3L8_out_joints = new List<int>();
            List<int> _L2_out_joints = new List<int>();


            List<int> _3L16_out_joints = new List<int>();
            List<int> _5L16_out_joints = new List<int>();
            List<int> _7L16_out_joints = new List<int>();



            //List<int> _L4_out_joints = new List<int>();
            //List<int> _deff_out_joints = new List<int>();

            List<int> _cross_joints = new List<int>();


            List<double> _X_joints = new List<double>();
            List<double> _Z_joints = new List<double>();

            for (i = 0; i < jn_col.Count; i++)
            {
                if (_X_joints.Contains(jn_col[i].X) == false) _X_joints.Add(jn_col[i].X);
                if (_Z_joints.Contains(jn_col[i].Z) == false) _Z_joints.Add(jn_col[i].Z);
            }
            //val = MyList.StringToDouble(txt_Ana_eff_depth.Text, -999.0);
            val = Deff;

            List<double> _X_min = new List<double>();
            List<double> _X_max = new List<double>();
            double x_max, x_min;
            double vvv = 99999999999999999;
            for (int zc = 0; zc < _Z_joints.Count; zc++)
            {
                x_min = vvv;
                x_max = -vvv;

                for (i = 0; i < jn_col.Count; i++)
                {
                    //if (_X_joints.Contains(jn_col[i].X) == false) _X_joints.Add(jn_col[i].X);
                    //if (_Z_joints.Contains(jn_col[i].Z) == false) _Z_joints.Add(jn_col[i].Z);

                    if (_Z_joints[zc] == jn_col[i].Z)
                    {
                        if (x_min > jn_col[i].X)
                            x_min = jn_col[i].X;
                        if (x_max < jn_col[i].X)
                            x_max = jn_col[i].X;
                    }
                }
                if (x_max != -vvv)
                    _X_max.Add(x_max);
                if (x_min != vvv)
                    _X_min.Add(x_min);
            }

            //val = MyList.StringToDouble(txt_Ana_eff_depth.Text, -999.0);
            val = Minor_Analysis.TotalLoad_Analysis.Analysis.Effective_Depth;
            //if (_X_joints.Contains(val))
            //{
            //    Bridge_Analysis.Effective_Depth = val;
            //}
            //else
            //{
            //    Bridge_Analysis.Effective_Depth = _X_joints.Count > 1 ? _X_joints[2] : 0.0; ;
            //}
            //double eff_dep = ;

            //_L_2_joints.Clear();

            double cant_wi_left = Minor_Analysis.Width_LeftCantilever;
            double cant_wi_right = Minor_Analysis.Width_RightCantilever;
            //Bridge_Analysis.Width_LeftCantilever = cant_wi;
            //Bridge_Analysis.Width_RightCantilever = _Z_joints[_Z_joints.Count - 1] - _Z_joints[_Z_joints.Count - 3];


            #region Find Joints
            for (i = 0; i < jn_col.Count; i++)
            {
                try
                {
                    if ((jn_col[i].Z >= cant_wi_left && jn_col[i].Z <= (W - cant_wi_right)) == false) continue;
                    x_min = _X_min[_Z_joints.IndexOf(jn_col[i].Z)];

                    if ((jn_col[i].X.ToString("0.0") == ((L / 2.0) + x_min).ToString("0.0")))
                    {
                        if (jn_col[i].Z >= cant_wi_left && jn_col[i].Z <= (W - cant_wi_right))
                            _L2_inn_joints.Add(jn_col[i].NodeNo);
                    }

                    if (jn_col[i].X.ToString("0.0") == ((3 * L / 8.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _3L8_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (3 * L / 8.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _3L8_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if (jn_col[i].X.ToString("0.0") == ((L / 8.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _L8_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (L / 8.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _L8_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if (jn_col[i].X.ToString("0.0") == ((L / 4.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _L4_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (L / 4.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _L4_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if ((jn_col[i].X.ToString("0.0") == (Minor_Analysis.Effective_Depth + x_min).ToString("0.0")))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _deff_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if ((jn_col[i].X.ToString("0.0") == (L - Minor_Analysis.Effective_Depth + x_min).ToString("0.0")))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _deff_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if (jn_col[i].X.ToString("0.0") == (x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _support_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == (L + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _support_out_joints.Add(jn_col[i].NodeNo);
                    }

                    #region 3L/16


                    if (jn_col[i].X.ToString("0.0") == ((3 * L / 16.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _3L16_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (3 * L / 16.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _3L16_out_joints.Add(jn_col[i].NodeNo);
                    }


                    #endregion 3L/16



                    #region 5L/16


                    if (jn_col[i].X.ToString("0.0") == ((5 * L / 16.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _5L16_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (5 * L / 16.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _5L16_out_joints.Add(jn_col[i].NodeNo);
                    }


                    #endregion 3L/16



                    #region 7L/16


                    if (jn_col[i].X.ToString("0.0") == ((7 * L / 16.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _7L16_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (7 * L / 16.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _7L16_out_joints.Add(jn_col[i].NodeNo);
                    }


                    #endregion 3L/16



                }
                catch (Exception ex) { MessageBox.Show(this, ""); }
            }

            #endregion Find Joints

            Result.Clear();
            Result.Add("");
            Result.Add("");
            //Result.Add("Analysis Result of RCC T-BEAM Bridge");
            Result.Add("");

            #region SHEAR FORCE
            List<double> SF_DL_Self_Weight_G1 = new List<double>();
            List<double> SF_DL_Self_Weight_G2 = new List<double>();

            List<double> SF_DL_Deck_Wet_Conc_G1 = new List<double>();
            List<double> SF_DL_Deck_Wet_Conc_G2 = new List<double>();

            List<double> SF_DL_Deck_Dry_Conc_G1 = new List<double>();
            List<double> SF_DL_Deck_Dry_Conc_G2 = new List<double>();

            List<double> SF_DL_Self_Deck_G1 = new List<double>();
            List<double> SF_DL_Self_Deck_G2 = new List<double>();
            List<double> SF_DL_Self_Deck_G3 = new List<double>();
            List<double> SF_DL_Self_Deck_G4 = new List<double>();

            List<double> SF_SIDL_Crash_Barrier_G1 = new List<double>();
            List<double> SF_SIDL_Crash_Barrier_G2 = new List<double>();
            List<double> SF_SIDL_Crash_Barrier_G3 = new List<double>();
            List<double> SF_SIDL_Crash_Barrier_G4 = new List<double>();

            List<double> SF_SIDL_Wearing_G1 = new List<double>();
            List<double> SF_SIDL_Wearing_G2 = new List<double>();
            List<double> SF_SIDL_Wearing_G3 = new List<double>();
            List<double> SF_SIDL_Wearing_G4 = new List<double>();


            List<double> SF_LL_1 = new List<double>();
            List<double> SF_LL_2 = new List<double>();
            List<double> SF_LL_3 = new List<double>();
            List<double> SF_LL_4 = new List<double>();
            List<double> SF_LL_5 = new List<double>();
            List<double> SF_LL_6 = new List<double>();

            MaxForce f = null;

            for (i = 1; i <= 6; i++)
            {
                #region Dead Load 1
                if (i == 1)
                {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(val);
                    SF_DL_Self_Weight_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(val);
                    SF_DL_Self_Weight_G2.Add(val);


                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(val);
                    SF_DL_Self_Weight_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(val);
                    SF_DL_Self_Weight_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(val);
                    SF_DL_Self_Weight_G2.Add(val);



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Weight_G1.Add(val);
                    SF_DL_Self_Weight_G2.Add(val);

                }
                #endregion Dead Load 1

                #region Dead Load 2
                else if (i == 2)
                {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G1.Add(val);
                    SF_DL_Deck_Wet_Conc_G2.Add(val);


                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G1.Add(val);
                    SF_DL_Deck_Wet_Conc_G2.Add(val);


                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G1.Add(val);
                    SF_DL_Deck_Wet_Conc_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G1.Add(val);
                    SF_DL_Deck_Wet_Conc_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G1.Add(val);
                    SF_DL_Deck_Wet_Conc_G2.Add(val);



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Wet_Conc_G1.Add(val);
                    SF_DL_Deck_Wet_Conc_G2.Add(val);

                }
                #endregion Dead Load 1

                #region Dead Load 3
                else if (i == 3)
                {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(val);
                    SF_DL_Deck_Dry_Conc_G2.Add(val);



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(val);
                    SF_DL_Deck_Dry_Conc_G2.Add(val);


                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(val);
                    SF_DL_Deck_Dry_Conc_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(val);
                    SF_DL_Deck_Dry_Conc_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(val);
                    SF_DL_Deck_Dry_Conc_G2.Add(val);



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Deck_Dry_Conc_G1.Add(val);
                    SF_DL_Deck_Dry_Conc_G2.Add(val);

                }
                #endregion Dead Load 3

                #region Dead Load 4
                else if (i == 4)
                {
                    #region Support
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[2], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[3], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G4.Add(val);

                    #endregion Support

                    #region Deff
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[2], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[3], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G4.Add(val);

                    #endregion Support



                    #region L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[2], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[3], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G4.Add(val);

                    #endregion  L / 8


                    #region L / 4
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[2], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[3], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G4.Add(val);

                    #endregion  L / 8

                    #region 3L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[2], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[3], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G4.Add(val);

                    #endregion  L / 8

                    #region L / 2
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[2], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[3], i);
                    val = f.Force;

                    SF_DL_Self_Deck_G4.Add(val);

                    #endregion  L / 8


                }
                #endregion Dead Load 4

                #region Dead Load 5
                else if (i == 5)
                {
                    #region Support
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(val);


                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G4.Add(val);

                    #endregion Support


                    #region Deff
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G4.Add(val);

                    #endregion Deff


                    #region L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G4.Add(val);

                    #endregion  L / 8

                    #region L / 4
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G4.Add(val);

                    #endregion  L / 8

                    #region 3L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G4.Add(val);

                    #endregion  L / 8

                    #region L / 2
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Crash_Barrier_G4.Add(val);

                    #endregion  L / 2

                }
                #endregion Dead Load 5

                #region Dead Load 6
                else if (i == 6)
                {
                    #region Support
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(val);




                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(val);
                    SF_SIDL_Wearing_G1.Add(val);



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_support_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G4.Add(val);

                    #endregion Support

                    #region Deff
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_deff_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G4.Add(val);

                    #endregion Deff

                    #region L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L8_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G4.Add(val);

                    #endregion  L / 8

                    #region L / 4
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L4_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G4.Add(val);

                    #endregion  L / 8

                    #region 3L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_3L8_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G4.Add(val);

                    #endregion  L / 8

                    #region L / 2
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[2], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_ShearForce(_L2_inn_joints[3], i);
                    val = f.Force;

                    SF_SIDL_Wearing_G4.Add(val);

                    #endregion  L / 2

                }
                #endregion Dead Load 5
            }

            List<int> tmp_jts = new List<int>();

            #region Live Load SHEAR Force
            #region Live Load 1

            tmp_jts.Clear();
            tmp_jts.AddRange(_support_inn_joints.ToArray());
            tmp_jts.AddRange(_support_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_ShearForce(_support_inn_joints, true);
            val = f.Force;
            SF_LL_1.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_deff_inn_joints.ToArray());
            tmp_jts.AddRange(_deff_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_1.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L8_inn_joints.ToArray());
            tmp_jts.AddRange(_L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_1.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_L4_inn_joints.ToArray());
            tmp_jts.AddRange(_L4_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_1.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_inn_joints.ToArray());
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_1.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L2_inn_joints.ToArray());
            tmp_jts.AddRange(_L2_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_1.Add(val);

            #endregion Support

            #region Live Load 2

            tmp_jts.Clear();
            tmp_jts.AddRange(_support_inn_joints.ToArray());
            tmp_jts.AddRange(_support_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_ShearForce(_support_inn_joints, true);
            val = f.Force;
            SF_LL_2.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_deff_inn_joints.ToArray());
            tmp_jts.AddRange(_deff_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L8_inn_joints.ToArray());
            tmp_jts.AddRange(_L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_2.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_L4_inn_joints.ToArray());
            tmp_jts.AddRange(_L4_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_inn_joints.ToArray());
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L2_inn_joints.ToArray());
            tmp_jts.AddRange(_L2_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_2.Add(val);

            #endregion Load 2

            #region Live Load 3

            tmp_jts.Clear();
            tmp_jts.AddRange(_support_inn_joints.ToArray());
            tmp_jts.AddRange(_support_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_ShearForce(_support_inn_joints, true);
            val = f.Force;
            SF_LL_3.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_deff_inn_joints.ToArray());
            tmp_jts.AddRange(_deff_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L8_inn_joints.ToArray());
            tmp_jts.AddRange(_L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_3.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_L4_inn_joints.ToArray());
            tmp_jts.AddRange(_L4_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_inn_joints.ToArray());
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L2_inn_joints.ToArray());
            tmp_jts.AddRange(_L2_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_3.Add(val);

            #endregion Load 2

            #region Live Load 4

            tmp_jts.Clear();
            tmp_jts.AddRange(_support_inn_joints.ToArray());
            tmp_jts.AddRange(_support_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_ShearForce(_support_inn_joints, true);
            val = f.Force;
            SF_LL_4.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_deff_inn_joints.ToArray());
            tmp_jts.AddRange(_deff_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L8_inn_joints.ToArray());
            tmp_jts.AddRange(_L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_4.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_L4_inn_joints.ToArray());
            tmp_jts.AddRange(_L4_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_inn_joints.ToArray());
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L2_inn_joints.ToArray());
            tmp_jts.AddRange(_L2_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_4.Add(val);

            #endregion Support

            #region Live Load 1

            tmp_jts.Clear();
            tmp_jts.AddRange(_support_inn_joints.ToArray());
            tmp_jts.AddRange(_support_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_ShearForce(_support_inn_joints, true);
            val = f.Force;
            SF_LL_5.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_deff_inn_joints.ToArray());
            tmp_jts.AddRange(_deff_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L8_inn_joints.ToArray());
            tmp_jts.AddRange(_L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_5.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_L4_inn_joints.ToArray());
            tmp_jts.AddRange(_L4_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_inn_joints.ToArray());
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L2_inn_joints.ToArray());
            tmp_jts.AddRange(_L2_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_5.Add(val);

            #endregion Support

            #region Live Load 1

            tmp_jts.Clear();
            tmp_jts.AddRange(_support_inn_joints.ToArray());
            tmp_jts.AddRange(_support_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_ShearForce(_support_inn_joints, true);
            val = f.Force;
            SF_LL_6.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_deff_inn_joints.ToArray());
            tmp_jts.AddRange(_deff_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L8_inn_joints.ToArray());
            tmp_jts.AddRange(_L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_6.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_L4_inn_joints.ToArray());
            tmp_jts.AddRange(_L4_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_inn_joints.ToArray());
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L2_inn_joints.ToArray());
            tmp_jts.AddRange(_L2_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_ShearForce(tmp_jts, true);
            val = f.Force;
            SF_LL_6.Add(val);

            #endregion Support


            #endregion Live Load SHEAR Force




            MyList.Array_Multiply_With(ref SF_DL_Self_Weight_G1, 10.0);
            MyList.Array_Multiply_With(ref SF_DL_Self_Weight_G2, 10.0);

            MyList.Array_Multiply_With(ref SF_DL_Deck_Wet_Conc_G1, 10.0);
            MyList.Array_Multiply_With(ref SF_DL_Deck_Wet_Conc_G2, 10.0);

            MyList.Array_Multiply_With(ref SF_DL_Deck_Dry_Conc_G1, 10.0);
            MyList.Array_Multiply_With(ref SF_DL_Deck_Dry_Conc_G2, 10.0);

            MyList.Array_Multiply_With(ref SF_DL_Self_Deck_G1, 10.0);
            MyList.Array_Multiply_With(ref SF_DL_Self_Deck_G2, 10.0);
            MyList.Array_Multiply_With(ref SF_DL_Self_Deck_G3, 10.0);
            MyList.Array_Multiply_With(ref SF_DL_Self_Deck_G4, 10.0);

            MyList.Array_Multiply_With(ref SF_SIDL_Crash_Barrier_G1, 10.0);
            MyList.Array_Multiply_With(ref SF_SIDL_Crash_Barrier_G2, 10.0);
            MyList.Array_Multiply_With(ref SF_SIDL_Crash_Barrier_G3, 10.0);
            MyList.Array_Multiply_With(ref SF_SIDL_Crash_Barrier_G4, 10.0);

            MyList.Array_Multiply_With(ref SF_SIDL_Wearing_G1, 10.0);
            MyList.Array_Multiply_With(ref SF_SIDL_Wearing_G2, 10.0);
            MyList.Array_Multiply_With(ref SF_SIDL_Wearing_G3, 10.0);
            MyList.Array_Multiply_With(ref SF_SIDL_Wearing_G4, 10.0);

            MyList.Array_Multiply_With(ref SF_LL_1, 10.0);
            MyList.Array_Multiply_With(ref SF_LL_2, 10.0);
            MyList.Array_Multiply_With(ref SF_LL_3, 10.0);
            MyList.Array_Multiply_With(ref SF_LL_4, 10.0);
            MyList.Array_Multiply_With(ref SF_LL_5, 10.0);
            MyList.Array_Multiply_With(ref SF_LL_6, 10.0);


            #endregion SHEAR FORCE

            #region Bending Moment
            List<double> BM_DL_Self_Weight_G1 = new List<double>();
            List<double> BM_DL_Self_Weight_G2 = new List<double>();

            List<double> BM_DL_Deck_Wet_Conc_G1 = new List<double>();
            List<double> BM_DL_Deck_Wet_Conc_G2 = new List<double>();

            List<double> BM_DL_Deck_Dry_Conc_G1 = new List<double>();
            List<double> BM_DL_Deck_Dry_Conc_G2 = new List<double>();

            List<double> BM_DL_Self_Deck_G1 = new List<double>();
            List<double> BM_DL_Self_Deck_G2 = new List<double>();
            List<double> BM_DL_Self_Deck_G3 = new List<double>();
            List<double> BM_DL_Self_Deck_G4 = new List<double>();

            List<double> BM_SIDL_Crash_Barrier_G1 = new List<double>();
            List<double> BM_SIDL_Crash_Barrier_G2 = new List<double>();
            List<double> BM_SIDL_Crash_Barrier_G3 = new List<double>();
            List<double> BM_SIDL_Crash_Barrier_G4 = new List<double>();

            List<double> BM_SIDL_Wearing_G1 = new List<double>();
            List<double> BM_SIDL_Wearing_G2 = new List<double>();
            List<double> BM_SIDL_Wearing_G3 = new List<double>();
            List<double> BM_SIDL_Wearing_G4 = new List<double>();



            List<double> BM_LL_1 = new List<double>();
            List<double> BM_LL_2 = new List<double>();
            List<double> BM_LL_3 = new List<double>();
            List<double> BM_LL_4 = new List<double>();
            List<double> BM_LL_5 = new List<double>();
            List<double> BM_LL_6 = new List<double>();


            for (i = 1; i <= 6; i++)
            {
                #region Dead Load 1
                if (i == 1)
                {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G1.Add(val);
                    BM_DL_Self_Weight_G2.Add(val);



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G1.Add(val);
                    BM_DL_Self_Weight_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G1.Add(val);
                    BM_DL_Self_Weight_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G1.Add(val);
                    BM_DL_Self_Weight_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G1.Add(val);
                    BM_DL_Self_Weight_G2.Add(val);



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Weight_G1.Add(val);
                    BM_DL_Self_Weight_G2.Add(val);

                }
                #endregion Dead Load 1

                #region Dead Load 2
                else if (i == 2)
                {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G1.Add(val);
                    BM_DL_Deck_Wet_Conc_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G1.Add(val);
                    BM_DL_Deck_Wet_Conc_G2.Add(val);


                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G1.Add(val);
                    BM_DL_Deck_Wet_Conc_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G1.Add(val);
                    BM_DL_Deck_Wet_Conc_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G1.Add(val);
                    BM_DL_Deck_Wet_Conc_G2.Add(val);



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Wet_Conc_G1.Add(val);
                    BM_DL_Deck_Wet_Conc_G2.Add(val);

                }
                #endregion Dead Load 1

                #region Dead Load 3
                else if (i == 3)
                {

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Dry_Conc_G1.Add(val);
                    BM_DL_Deck_Dry_Conc_G2.Add(val);



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Dry_Conc_G1.Add(val);
                    BM_DL_Deck_Dry_Conc_G2.Add(val);



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Dry_Conc_G1.Add(val);
                    BM_DL_Deck_Dry_Conc_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Dry_Conc_G1.Add(val);
                    BM_DL_Deck_Dry_Conc_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Dry_Conc_G1.Add(val);
                    BM_DL_Deck_Dry_Conc_G2.Add(val);



                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Deck_Dry_Conc_G1.Add(val);
                    BM_DL_Deck_Dry_Conc_G2.Add(val);

                }
                #endregion Dead Load 3

                #region Dead Load 4
                else if (i == 4)
                {
                    #region Support
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[2], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[3], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G4.Add(val);

                    #endregion Support



                    #region Deff
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[2], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[3], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G4.Add(val);

                    #endregion Deff




                    #region L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[2], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[3], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G4.Add(val);

                    #endregion  L / 8


                    #region L / 4
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[2], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[3], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G4.Add(val);

                    #endregion  L / 8

                    #region 3L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[2], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[3], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G4.Add(val);

                    #endregion  L / 8

                    #region L / 2
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[2], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[3], i);
                    val = f.Force;

                    BM_DL_Self_Deck_G4.Add(val);

                    #endregion  L / 8


                }
                #endregion Dead Load 4

                #region Dead Load 5
                else if (i == 5)
                {
                    #region Support
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[2], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[3], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G4.Add(val);

                    #endregion Support



                    #region Deff
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[2], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[3], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G4.Add(val);

                    #endregion Deff


                    #region L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[2], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[3], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G4.Add(val);

                    #endregion  L / 8

                    #region L / 4
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[2], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[3], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G4.Add(val);

                    #endregion  L / 8

                    #region 3L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[2], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[3], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G4.Add(val);

                    #endregion  L / 8

                    #region L / 2
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[2], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[3], i);
                    val = f.Force;

                    BM_SIDL_Crash_Barrier_G4.Add(val);

                    #endregion  L / 2

                }
                #endregion Dead Load 5

                #region Dead Load 5
                else if (i == 6)
                {
                    #region Support
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[2], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_support_inn_joints[3], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G4.Add(val);

                    #endregion Support



                    #region Deff
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[2], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_deff_inn_joints[3], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G4.Add(val);

                    #endregion Deff


                    #region L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[2], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L8_inn_joints[3], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G4.Add(val);

                    #endregion  L / 8

                    #region L / 4
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[2], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L4_inn_joints[3], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G4.Add(val);

                    #endregion  L / 8

                    #region 3L / 8
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[2], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_3L8_inn_joints[3], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G4.Add(val);

                    #endregion  L / 8

                    #region L / 2
                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[0], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G1.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[1], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G2.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[2], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G3.Add(val);

                    f = Minor_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(_L2_inn_joints[3], i);
                    val = f.Force;

                    BM_SIDL_Wearing_G4.Add(val);

                    #endregion  L / 2

                }
                #endregion Dead Load 5
            }


            #region Live Load SHEAR Force
            #region Live Load 1

            tmp_jts.Clear();
            tmp_jts.AddRange(_support_inn_joints.ToArray());
            tmp_jts.AddRange(_support_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_MomentForce(_support_inn_joints, true);
            val = f.Force;
            BM_LL_1.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_deff_inn_joints.ToArray());
            tmp_jts.AddRange(_deff_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_1.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L8_inn_joints.ToArray());
            tmp_jts.AddRange(_L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_1.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_L4_inn_joints.ToArray());
            tmp_jts.AddRange(_L4_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_1.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_inn_joints.ToArray());
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_1.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L2_inn_joints.ToArray());
            tmp_jts.AddRange(_L2_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_1.Add(val);

            #endregion Support

            #region Live Load 2

            tmp_jts.Clear();
            tmp_jts.AddRange(_support_inn_joints.ToArray());
            tmp_jts.AddRange(_support_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_MomentForce(_support_inn_joints, true);
            val = f.Force;
            BM_LL_2.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_deff_inn_joints.ToArray());
            tmp_jts.AddRange(_deff_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L8_inn_joints.ToArray());
            tmp_jts.AddRange(_L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_2.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_L4_inn_joints.ToArray());
            tmp_jts.AddRange(_L4_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_inn_joints.ToArray());
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L2_inn_joints.ToArray());
            tmp_jts.AddRange(_L2_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_2.Add(val);

            #endregion Load 2

            #region Live Load 3

            tmp_jts.Clear();
            tmp_jts.AddRange(_support_inn_joints.ToArray());
            tmp_jts.AddRange(_support_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_MomentForce(_support_inn_joints, true);
            val = f.Force;
            BM_LL_3.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_deff_inn_joints.ToArray());
            tmp_jts.AddRange(_deff_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L8_inn_joints.ToArray());
            tmp_jts.AddRange(_L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_3.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_L4_inn_joints.ToArray());
            tmp_jts.AddRange(_L4_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_inn_joints.ToArray());
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L2_inn_joints.ToArray());
            tmp_jts.AddRange(_L2_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_3.Add(val);

            #endregion Load 2

            #region Live Load 4

            tmp_jts.Clear();
            tmp_jts.AddRange(_support_inn_joints.ToArray());
            tmp_jts.AddRange(_support_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_MomentForce(_support_inn_joints, true);
            val = f.Force;
            BM_LL_4.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_deff_inn_joints.ToArray());
            tmp_jts.AddRange(_deff_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L8_inn_joints.ToArray());
            tmp_jts.AddRange(_L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_4.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_L4_inn_joints.ToArray());
            tmp_jts.AddRange(_L4_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_inn_joints.ToArray());
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L2_inn_joints.ToArray());
            tmp_jts.AddRange(_L2_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_4.Add(val);

            #endregion Support

            #region Live Load 1

            tmp_jts.Clear();
            tmp_jts.AddRange(_support_inn_joints.ToArray());
            tmp_jts.AddRange(_support_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_MomentForce(_support_inn_joints, true);
            val = f.Force;
            BM_LL_5.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_deff_inn_joints.ToArray());
            tmp_jts.AddRange(_deff_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L8_inn_joints.ToArray());
            tmp_jts.AddRange(_L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_5.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_L4_inn_joints.ToArray());
            tmp_jts.AddRange(_L4_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_inn_joints.ToArray());
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L2_inn_joints.ToArray());
            tmp_jts.AddRange(_L2_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_5.Add(val);

            #endregion Support

            #region Live Load 1

            tmp_jts.Clear();
            tmp_jts.AddRange(_support_inn_joints.ToArray());
            tmp_jts.AddRange(_support_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_MomentForce(_support_inn_joints, true);
            val = f.Force;
            BM_LL_6.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_deff_inn_joints.ToArray());
            tmp_jts.AddRange(_deff_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L8_inn_joints.ToArray());
            tmp_jts.AddRange(_L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_6.Add(val);

            tmp_jts.Clear();
            tmp_jts.AddRange(_L4_inn_joints.ToArray());
            tmp_jts.AddRange(_L4_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_inn_joints.ToArray());
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_L2_inn_joints.ToArray());
            tmp_jts.AddRange(_L2_out_joints.ToArray());
            f = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_MomentForce(tmp_jts, true);
            val = f.Force;
            BM_LL_6.Add(val);

            #endregion Support


            #region Deck Slab Data
            //List<double> 
            #endregion Deck Slab Data


            #endregion Live Load SHEAR Force




            MyList.Array_Multiply_With(ref BM_DL_Self_Weight_G1, 10.0);
            MyList.Array_Multiply_With(ref BM_DL_Self_Weight_G2, 10.0);

            MyList.Array_Multiply_With(ref BM_DL_Deck_Wet_Conc_G1, 10.0);
            MyList.Array_Multiply_With(ref BM_DL_Deck_Wet_Conc_G2, 10.0);

            MyList.Array_Multiply_With(ref BM_DL_Deck_Dry_Conc_G1, 10.0);
            MyList.Array_Multiply_With(ref BM_DL_Deck_Dry_Conc_G2, 10.0);

            MyList.Array_Multiply_With(ref BM_DL_Self_Deck_G1, 10.0);
            MyList.Array_Multiply_With(ref BM_DL_Self_Deck_G2, 10.0);
            MyList.Array_Multiply_With(ref BM_DL_Self_Deck_G3, 10.0);
            MyList.Array_Multiply_With(ref BM_DL_Self_Deck_G4, 10.0);

            MyList.Array_Multiply_With(ref BM_SIDL_Crash_Barrier_G1, 10.0);
            MyList.Array_Multiply_With(ref BM_SIDL_Crash_Barrier_G2, 10.0);
            MyList.Array_Multiply_With(ref BM_SIDL_Crash_Barrier_G3, 10.0);
            MyList.Array_Multiply_With(ref BM_SIDL_Crash_Barrier_G4, 10.0);

            MyList.Array_Multiply_With(ref BM_SIDL_Wearing_G1, 10.0);
            MyList.Array_Multiply_With(ref BM_SIDL_Wearing_G2, 10.0);
            MyList.Array_Multiply_With(ref BM_SIDL_Wearing_G3, 10.0);
            MyList.Array_Multiply_With(ref BM_SIDL_Wearing_G4, 10.0);



            MyList.Array_Multiply_With(ref BM_LL_1, 10.0);
            MyList.Array_Multiply_With(ref BM_LL_2, 10.0);
            MyList.Array_Multiply_With(ref BM_LL_3, 10.0);
            MyList.Array_Multiply_With(ref BM_LL_4, 10.0);
            MyList.Array_Multiply_With(ref BM_LL_5, 10.0);
            MyList.Array_Multiply_With(ref BM_LL_6, 10.0);



            #endregion Bending Moment


            #region Cross Girder Data


            double crss_hog, crss_sag, crss_frc;

            _cross_joints.Clear();

            for (i = 0; i < jn_col.Count; i++)
            {
                _cross_joints.Add(jn_col[i].NodeNo);
            }


            List<double> lst_crss_hog = new List<double>();
            List<double> lst_crss_sag = new List<double>();
            List<double> lst_crss_frc = new List<double>();

            crss_hog = Minor_Analysis.TotalLoad_Analysis.GetJoint_Max_Hogging(_cross_joints, true);
            crss_sag = Minor_Analysis.TotalLoad_Analysis.GetJoint_Max_Sagging(_cross_joints, true);
            crss_frc = Minor_Analysis.TotalLoad_Analysis.GetJoint_ShearForce(_cross_joints);

            lst_crss_hog.Add(crss_hog);
            lst_crss_sag.Add(crss_sag);
            lst_crss_frc.Add(crss_frc);

            crss_hog = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Hogging(_cross_joints, true);
            crss_sag = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Sagging(_cross_joints, true);
            crss_frc = Minor_Analysis.LiveLoad_1_Analysis.GetJoint_ShearForce(_cross_joints);

            lst_crss_hog.Add(crss_hog);
            lst_crss_sag.Add(crss_sag);
            lst_crss_frc.Add(crss_frc);


            crss_hog = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Hogging(_cross_joints, true);
            crss_sag = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Sagging(_cross_joints, true);
            crss_frc = Minor_Analysis.LiveLoad_2_Analysis.GetJoint_ShearForce(_cross_joints);

            lst_crss_hog.Add(crss_hog);
            lst_crss_sag.Add(crss_sag);
            lst_crss_frc.Add(crss_frc);


            crss_hog = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Hogging(_cross_joints, true);
            crss_sag = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Sagging(_cross_joints, true);
            crss_frc = Minor_Analysis.LiveLoad_3_Analysis.GetJoint_ShearForce(_cross_joints);

            lst_crss_hog.Add(crss_hog);
            lst_crss_sag.Add(crss_sag);
            lst_crss_frc.Add(crss_frc);


            crss_hog = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Hogging(_cross_joints, true);
            crss_sag = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Sagging(_cross_joints, true);
            crss_frc = Minor_Analysis.LiveLoad_4_Analysis.GetJoint_ShearForce(_cross_joints);

            lst_crss_hog.Add(crss_hog);
            lst_crss_sag.Add(crss_sag);
            lst_crss_frc.Add(crss_frc);


            crss_hog = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Hogging(_cross_joints, true);
            crss_sag = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Sagging(_cross_joints, true);
            crss_frc = Minor_Analysis.LiveLoad_5_Analysis.GetJoint_ShearForce(_cross_joints);

            lst_crss_hog.Add(crss_hog);
            lst_crss_sag.Add(crss_sag);
            lst_crss_frc.Add(crss_frc);


            crss_hog = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Hogging(_cross_joints, true);
            crss_sag = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Sagging(_cross_joints, true);
            crss_frc = Minor_Analysis.LiveLoad_6_Analysis.GetJoint_ShearForce(_cross_joints);

            lst_crss_hog.Add(crss_hog);
            lst_crss_sag.Add(crss_sag);
            lst_crss_frc.Add(crss_frc);


            lst_crss_hog.Sort();
            lst_crss_hog.Reverse();
            lst_crss_sag.Sort();
            //lst_crss_sag.Reverse();
            lst_crss_frc.Sort();
            lst_crss_frc.Reverse();

            if (dgv_cross_user_input.RowCount > 7)
            {
                dgv_cross_user_input[1, dgv_cross_user_input.RowCount - 1].Value = Math.Abs(lst_crss_frc[0]).ToString("f3");
                dgv_cross_user_input[1, dgv_cross_user_input.RowCount - 2].Value = Math.Abs(lst_crss_sag[0]).ToString("f3");
                dgv_cross_user_input[1, dgv_cross_user_input.RowCount - 3].Value = Math.Abs(lst_crss_hog[0]).ToString("f3");
            }
            #endregion Cross Girder Data

            val = 0.0;


            string format = "{0,-30} {1,12} {2,10:f3} {3,15:f3} {4,10:f3} {5,10:f3} {6,10:f3} {7,10:f3}";

            Result.Add(string.Format(""));

            #region Summary 1

            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------"));
            Result.Add(string.Format("RCC T GIRDER (LIMIT STATE METHOD) ANALYSIS RESULTS "));
            Result.Add(string.Format("---------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of BM and SF for different load cases ( Unfactored)"));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of B.M. & S.F. due to Dead Load (Forces due to Self weight of girder) kN-m"));
            Result.Add(string.Format("------------------------------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("     Girder No                  Components    Support       Web widening    1/8th     1/4th      3/8th        Mid "));
            Result.Add(string.Format("                                                                 end        span      span       span        span"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));


            Result.Add(string.Format(format, "G1 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Self_Weight_G1[0],
                                                BM_DL_Self_Weight_G1[1],
                                                BM_DL_Self_Weight_G1[2],
                                                BM_DL_Self_Weight_G1[3],
                                                BM_DL_Self_Weight_G1[4],
                                                BM_DL_Self_Weight_G1[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN", SF_DL_Self_Weight_G1[0],
                                                SF_DL_Self_Weight_G1[1],
                                                SF_DL_Self_Weight_G1[2],
                                                SF_DL_Self_Weight_G1[3],
                                                SF_DL_Self_Weight_G1[4],
                                                SF_DL_Self_Weight_G1[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G2 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Self_Weight_G2[0],
                                                BM_DL_Self_Weight_G2[1],
                                                BM_DL_Self_Weight_G2[2],
                                                BM_DL_Self_Weight_G2[3],
                                                BM_DL_Self_Weight_G2[4],
                                                BM_DL_Self_Weight_G2[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Self_Weight_G2[0],
                                                SF_DL_Self_Weight_G2[1],
                                                SF_DL_Self_Weight_G2[2],
                                                SF_DL_Self_Weight_G2[3],
                                                SF_DL_Self_Weight_G2[4],
                                                SF_DL_Self_Weight_G2[5]));

            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            #endregion Summary 1

            #region Summary 2

            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of B.M. & S.F. due to Dead Load (Forces due to Deck slab Wet concrete and Shuttering load)"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("     Girder No                  Components    Support       Web widening    1/8th     1/4th      3/8th        Mid "));
            Result.Add(string.Format("                                                                 end        span      span       span        span"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(format, "G1 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Deck_Wet_Conc_G1[0],
                                                BM_DL_Deck_Wet_Conc_G1[1],
                                                BM_DL_Deck_Wet_Conc_G1[2],
                                                BM_DL_Deck_Wet_Conc_G1[3],
                                                BM_DL_Deck_Wet_Conc_G1[4],
                                                BM_DL_Deck_Wet_Conc_G1[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Deck_Wet_Conc_G1[0],
                                                SF_DL_Deck_Wet_Conc_G1[1],
                                                SF_DL_Deck_Wet_Conc_G1[2],
                                                SF_DL_Deck_Wet_Conc_G1[3],
                                                SF_DL_Deck_Wet_Conc_G1[4],
                                                SF_DL_Deck_Wet_Conc_G1[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G2 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Deck_Wet_Conc_G2[0],
                                                BM_DL_Deck_Wet_Conc_G2[1],
                                                BM_DL_Deck_Wet_Conc_G2[2],
                                                BM_DL_Deck_Wet_Conc_G2[3],
                                                BM_DL_Deck_Wet_Conc_G2[4],
                                                BM_DL_Deck_Wet_Conc_G2[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Deck_Wet_Conc_G2[0],
                                                SF_DL_Deck_Wet_Conc_G2[1],
                                                SF_DL_Deck_Wet_Conc_G2[2],
                                                SF_DL_Deck_Wet_Conc_G2[3],
                                                SF_DL_Deck_Wet_Conc_G2[4],
                                                SF_DL_Deck_Wet_Conc_G2[5]));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            #endregion Summary 2

            #region Summary 3

            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of B.M. & S.F. due to Deshutering load"));
            Result.Add(string.Format("-----------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("     Girder No                  Components    Support       Web widening    1/8th     1/4th      3/8th        Mid "));
            Result.Add(string.Format("                                                                 end        span      span       span        span"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(format, "G1 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Deck_Dry_Conc_G1[0],
                                                BM_DL_Deck_Dry_Conc_G1[1],
                                                BM_DL_Deck_Dry_Conc_G1[2],
                                                BM_DL_Deck_Dry_Conc_G1[3],
                                                BM_DL_Deck_Dry_Conc_G1[4],
                                                BM_DL_Deck_Dry_Conc_G1[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Deck_Dry_Conc_G1[0],
                                                SF_DL_Deck_Dry_Conc_G1[1],
                                                SF_DL_Deck_Dry_Conc_G1[2],
                                                SF_DL_Deck_Dry_Conc_G1[3],
                                                SF_DL_Deck_Dry_Conc_G1[4],
                                                SF_DL_Deck_Dry_Conc_G1[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G2 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Deck_Dry_Conc_G2[0],
                                                BM_DL_Deck_Dry_Conc_G2[1],
                                                BM_DL_Deck_Dry_Conc_G2[2],
                                                BM_DL_Deck_Dry_Conc_G2[3],
                                                BM_DL_Deck_Dry_Conc_G2[4],
                                                BM_DL_Deck_Dry_Conc_G2[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Deck_Dry_Conc_G2[0],
                                                SF_DL_Deck_Dry_Conc_G2[1],
                                                SF_DL_Deck_Dry_Conc_G2[2],
                                                SF_DL_Deck_Dry_Conc_G2[3],
                                                SF_DL_Deck_Dry_Conc_G2[4],
                                                SF_DL_Deck_Dry_Conc_G2[5]));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            #endregion Summary 3

            #region Summary 4

            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of B.M. & S.F. per girder due to Dead Load (Forces due to Self weight of girder,Deck slab dry concrete)"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("     Girder No                  Components    Support       Web widening    1/8th     1/4th      3/8th        Mid "));
            Result.Add(string.Format("                                                                 end        span      span       span        span"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(format, "G1 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Self_Deck_G1[0],
                                                BM_DL_Self_Deck_G1[1],
                                                BM_DL_Self_Deck_G1[2],
                                                BM_DL_Self_Deck_G1[3],
                                                BM_DL_Self_Deck_G1[4],
                                                BM_DL_Self_Deck_G1[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Self_Deck_G1[0],
                                                SF_DL_Self_Deck_G1[1],
                                                SF_DL_Self_Deck_G1[2],
                                                SF_DL_Self_Deck_G1[3],
                                                SF_DL_Self_Deck_G1[4],
                                                SF_DL_Self_Deck_G1[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G2 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Self_Deck_G2[0],
                                                BM_DL_Self_Deck_G2[1],
                                                BM_DL_Self_Deck_G2[2],
                                                BM_DL_Self_Deck_G2[3],
                                                BM_DL_Self_Deck_G2[4],
                                                BM_DL_Self_Deck_G2[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Self_Deck_G2[0],
                                                SF_DL_Self_Deck_G2[1],
                                                SF_DL_Self_Deck_G2[2],
                                                SF_DL_Self_Deck_G2[3],
                                                SF_DL_Self_Deck_G2[4],
                                                SF_DL_Self_Deck_G2[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G3 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Self_Deck_G3[0],
                                                BM_DL_Self_Deck_G3[1],
                                                BM_DL_Self_Deck_G3[2],
                                                BM_DL_Self_Deck_G3[3],
                                                BM_DL_Self_Deck_G3[4],
                                                BM_DL_Self_Deck_G3[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Self_Deck_G3[0],
                                                SF_DL_Self_Deck_G3[1],
                                                SF_DL_Self_Deck_G3[2],
                                                SF_DL_Self_Deck_G3[3],
                                                SF_DL_Self_Deck_G3[4],
                                                SF_DL_Self_Deck_G3[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G4 ( Analysis Long Member )", "BM in kN-m",
                                                BM_DL_Self_Deck_G4[0],
                                                BM_DL_Self_Deck_G4[1],
                                                BM_DL_Self_Deck_G4[2],
                                                BM_DL_Self_Deck_G4[3],
                                                BM_DL_Self_Deck_G4[4],
                                                BM_DL_Self_Deck_G4[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_DL_Self_Deck_G4[0],
                                                SF_DL_Self_Deck_G4[1],
                                                SF_DL_Self_Deck_G4[2],
                                                SF_DL_Self_Deck_G4[3],
                                                SF_DL_Self_Deck_G4[4],
                                                SF_DL_Self_Deck_G4[5]));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            #endregion Summary 4

            #region Summary 5

            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of B.M. & S.F. per girder due to SIDL(Crash barrier)"));
            Result.Add(string.Format("------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("     Girder No                  Components    Support       Web widening    1/8th     1/4th      3/8th        Mid "));
            Result.Add(string.Format("                                                                 end        span      span       span        span"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(format, "G1 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Crash_Barrier_G1[0],
                                                BM_SIDL_Crash_Barrier_G1[1],
                                                BM_SIDL_Crash_Barrier_G1[2],
                                                BM_SIDL_Crash_Barrier_G1[3],
                                                BM_SIDL_Crash_Barrier_G1[4],
                                                BM_SIDL_Crash_Barrier_G1[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Crash_Barrier_G1[0],
                                                SF_SIDL_Crash_Barrier_G1[1],
                                                SF_SIDL_Crash_Barrier_G1[2],
                                                SF_SIDL_Crash_Barrier_G1[3],
                                                SF_SIDL_Crash_Barrier_G1[4],
                                                SF_SIDL_Crash_Barrier_G1[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G2 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Crash_Barrier_G2[0],
                                                BM_SIDL_Crash_Barrier_G2[1],
                                                BM_SIDL_Crash_Barrier_G2[2],
                                                BM_SIDL_Crash_Barrier_G2[3],
                                                BM_SIDL_Crash_Barrier_G2[4],
                                                BM_SIDL_Crash_Barrier_G2[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Crash_Barrier_G2[0],
                                                SF_SIDL_Crash_Barrier_G2[1],
                                                SF_SIDL_Crash_Barrier_G2[2],
                                                SF_SIDL_Crash_Barrier_G2[3],
                                                SF_SIDL_Crash_Barrier_G2[4],
                                                SF_SIDL_Crash_Barrier_G2[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G3 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Crash_Barrier_G3[0],
                                                BM_SIDL_Crash_Barrier_G3[1],
                                                BM_SIDL_Crash_Barrier_G3[2],
                                                BM_SIDL_Crash_Barrier_G3[3],
                                                BM_SIDL_Crash_Barrier_G3[4],
                                                BM_SIDL_Crash_Barrier_G3[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Crash_Barrier_G3[0],
                                                SF_SIDL_Crash_Barrier_G3[1],
                                                SF_SIDL_Crash_Barrier_G3[2],
                                                SF_SIDL_Crash_Barrier_G3[3],
                                                SF_SIDL_Crash_Barrier_G3[4],
                                                SF_SIDL_Crash_Barrier_G3[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G4 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Crash_Barrier_G4[0],
                                                BM_SIDL_Crash_Barrier_G4[1],
                                                BM_SIDL_Crash_Barrier_G4[2],
                                                BM_SIDL_Crash_Barrier_G4[3],
                                                BM_SIDL_Crash_Barrier_G4[4],
                                                BM_SIDL_Crash_Barrier_G4[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Crash_Barrier_G4[0],
                                                SF_SIDL_Crash_Barrier_G4[1],
                                                SF_SIDL_Crash_Barrier_G4[2],
                                                SF_SIDL_Crash_Barrier_G4[3],
                                                SF_SIDL_Crash_Barrier_G4[4],
                                                SF_SIDL_Crash_Barrier_G4[5]));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            #endregion Summary 5

            #region Summary 6

            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of B.M. & S.F. per girder due to SIDL(Wearing coat)"));
            Result.Add(string.Format("------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("     Girder No                  Components    Support       Web widening    1/8th     1/4th      3/8th        Mid "));
            Result.Add(string.Format("                                                                 end        span      span       span        span"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(format, "G1 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Wearing_G1[0],
                                                BM_SIDL_Wearing_G1[1],
                                                BM_SIDL_Wearing_G1[2],
                                                BM_SIDL_Wearing_G1[3],
                                                BM_SIDL_Wearing_G1[4],
                                                BM_SIDL_Wearing_G1[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Wearing_G1[0],
                                                SF_SIDL_Wearing_G1[1],
                                                SF_SIDL_Wearing_G1[2],
                                                SF_SIDL_Wearing_G1[3],
                                                SF_SIDL_Wearing_G1[4],
                                                SF_SIDL_Wearing_G1[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G2 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Wearing_G2[0],
                                                BM_SIDL_Wearing_G2[1],
                                                BM_SIDL_Wearing_G2[2],
                                                BM_SIDL_Wearing_G2[3],
                                                BM_SIDL_Wearing_G2[4],
                                                BM_SIDL_Wearing_G2[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Wearing_G2[0],
                                                SF_SIDL_Wearing_G2[1],
                                                SF_SIDL_Wearing_G2[2],
                                                SF_SIDL_Wearing_G2[3],
                                                SF_SIDL_Wearing_G2[4],
                                                SF_SIDL_Wearing_G2[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G3 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Wearing_G3[0],
                                                BM_SIDL_Wearing_G3[1],
                                                BM_SIDL_Wearing_G3[2],
                                                BM_SIDL_Wearing_G3[3],
                                                BM_SIDL_Wearing_G3[4],
                                                BM_SIDL_Wearing_G3[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Wearing_G3[0],
                                                SF_SIDL_Wearing_G3[1],
                                                SF_SIDL_Wearing_G3[2],
                                                SF_SIDL_Wearing_G3[3],
                                                SF_SIDL_Wearing_G3[4],
                                                SF_SIDL_Wearing_G3[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "G4 ( Analysis Long Member )", "BM in kN-m",
                                                BM_SIDL_Wearing_G4[0],
                                                BM_SIDL_Wearing_G4[1],
                                                BM_SIDL_Wearing_G4[2],
                                                BM_SIDL_Wearing_G4[3],
                                                BM_SIDL_Wearing_G4[4],
                                                BM_SIDL_Wearing_G4[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_SIDL_Wearing_G4[0],
                                                SF_SIDL_Wearing_G4[1],
                                                SF_SIDL_Wearing_G4[2],
                                                SF_SIDL_Wearing_G4[3],
                                                SF_SIDL_Wearing_G4[4],
                                                SF_SIDL_Wearing_G4[5]));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            #endregion Summary 6

            #region Summary 7

            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("Summary of B.M. & S.F. per girder due to Live Load"));
            Result.Add(string.Format("---------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format("     Girder No                  Components    Support       Web widening    1/8th     1/4th      3/8th        Mid "));
            Result.Add(string.Format("                                                                 end        span      span       span        span"));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(format, "LL1(1 Lane TYPE 3)", "BM in kN-m",
                                                BM_LL_1[0],
                                                BM_LL_1[1],
                                                BM_LL_1[2],
                                                BM_LL_1[3],
                                                BM_LL_1[4],
                                                BM_LL_1[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_LL_1[0],
                                                SF_LL_1[1],
                                                SF_LL_1[2],
                                                SF_LL_1[3],
                                                SF_LL_1[4],
                                                SF_LL_1[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "LL2(1 Lane TYPE 1)", "BM in kN-m",
                                               BM_LL_2[0],
                                               BM_LL_2[1],
                                               BM_LL_2[2],
                                               BM_LL_2[3],
                                               BM_LL_2[4],
                                               BM_LL_2[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_LL_2[0],
                                                SF_LL_2[1],
                                                SF_LL_2[2],
                                                SF_LL_2[3],
                                                SF_LL_2[4],
                                                SF_LL_2[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "LL3(2 Lane TYPE 1)", "BM in kN-m",
                                               BM_LL_3[0],
                                               BM_LL_3[1],
                                               BM_LL_3[2],
                                               BM_LL_3[3],
                                               BM_LL_3[4],
                                               BM_LL_3[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_LL_3[0],
                                                SF_LL_3[1],
                                                SF_LL_3[2],
                                                SF_LL_3[3],
                                                SF_LL_3[4],
                                                SF_LL_3[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "LL4(3 Lane TYPE 1)", "BM in kN-m",
                                               BM_LL_4[0],
                                               BM_LL_4[1],
                                               BM_LL_4[2],
                                               BM_LL_4[3],
                                               BM_LL_4[4],
                                               BM_LL_4[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_LL_4[0],
                                                SF_LL_4[1],
                                                SF_LL_4[2],
                                                SF_LL_4[3],
                                                SF_LL_4[4],
                                                SF_LL_4[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "LL5(1L TYPE 1 + 1L TYPE 3)", "BM in kN-m",
                                               BM_LL_5[0],
                                               BM_LL_5[1],
                                               BM_LL_5[2],
                                               BM_LL_5[3],
                                               BM_LL_5[4],
                                               BM_LL_5[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_LL_5[0],
                                                SF_LL_5[1],
                                                SF_LL_5[2],
                                                SF_LL_5[3],
                                                SF_LL_5[4],
                                                SF_LL_5[5]));
            Result.Add(string.Format(""));
            Result.Add(string.Format(format, "LL6(1L TYPE 3 + 1L TYPE 1)", "BM in kN-m",
                                               BM_LL_6[0],
                                               BM_LL_6[1],
                                               BM_LL_6[2],
                                               BM_LL_6[3],
                                               BM_LL_6[4],
                                               BM_LL_6[5]));
            //Result.Add(string.Format(""));
            Result.Add(string.Format(format, "", "SF in kN",
                                                SF_LL_6[0],
                                                SF_LL_6[1],
                                                SF_LL_6[2],
                                                SF_LL_6[3],
                                                SF_LL_6[4],
                                                SF_LL_6[5]));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------"));

            #endregion Summary 7



            List<int> outer_joints = new List<int>();
            List<int> outer_joints_right = new List<int>();
            if (_support_inn_joints.Count > 0)
            {
                outer_joints.Add(_support_inn_joints[0]);
                outer_joints_right.Add(_support_inn_joints[_support_inn_joints.Count - 1]);
            }


            if (_L8_inn_joints.Count > 0)
                outer_joints.Add(_L8_inn_joints[0]);
            outer_joints_right.Add(_L8_inn_joints[_L8_inn_joints.Count - 1]);

            if (_3L16_inn_joints.Count > 0)
            {
                outer_joints.Add(_3L16_inn_joints[0]);
                outer_joints_right.Add(_3L16_inn_joints[_3L16_inn_joints.Count - 1]);
            }

            if (_L4_inn_joints.Count > 0)
            {
                outer_joints.Add(_L4_inn_joints[0]);
                outer_joints_right.Add(_L4_inn_joints[_L4_inn_joints.Count - 1]);
            }
            if (_5L16_inn_joints.Count > 0)
            {
                outer_joints.Add(_5L16_inn_joints[0]);
                outer_joints_right.Add(_5L16_inn_joints[_5L16_inn_joints.Count - 1]);
            }
            if (_3L8_inn_joints.Count > 0)
            {
                outer_joints.Add(_3L8_inn_joints[0]);
                outer_joints_right.Add(_3L8_inn_joints[_3L8_inn_joints.Count - 1]);
            }
            if (_7L16_inn_joints.Count > 0)
            {
                outer_joints.Add(_7L16_inn_joints[0]);
                outer_joints_right.Add(_7L16_inn_joints[_7L16_inn_joints.Count - 1]);
            }
            if (_L2_inn_joints.Count > 0)
            {
                outer_joints.Add(_L2_inn_joints[0]);
                outer_joints_right.Add(_L2_inn_joints[_L2_inn_joints.Count - 1]);
            }


            if (_7L16_out_joints.Count > 0)
            {
                outer_joints.Add(_7L16_out_joints[0]);
                outer_joints_right.Add(_7L16_out_joints[_7L16_out_joints.Count - 1]);
            }



            if (_3L8_out_joints.Count > 0)
            {
                outer_joints.Add(_3L8_out_joints[0]);
                outer_joints_right.Add(_3L8_out_joints[_3L8_out_joints.Count - 1]);
            }



            if (_5L16_out_joints.Count > 0)
            {
                outer_joints.Add(_5L16_out_joints[0]);
                outer_joints_right.Add(_5L16_out_joints[_5L16_out_joints.Count - 1]);
            }


            if (_L4_out_joints.Count > 0)
            {
                outer_joints.Add(_L4_out_joints[0]);
                outer_joints_right.Add(_L4_out_joints[_L4_out_joints.Count - 1]);
            }


            if (_3L16_out_joints.Count > 0)
            {
                outer_joints.Add(_3L16_out_joints[0]);
                outer_joints_right.Add(_3L16_out_joints[_3L16_out_joints.Count - 1]);
            }


            if (_L8_out_joints.Count > 0)
            {
                outer_joints.Add(_L8_out_joints[0]);
                outer_joints_right.Add(_L8_out_joints[_L8_out_joints.Count - 1]);
            }


            if (_support_out_joints.Count > 0)
            {
                outer_joints.Add(_support_out_joints[0]);
                outer_joints_right.Add(_support_out_joints[_support_out_joints.Count - 1]);
            }





            iApp.Progress_ON("Reading Maximum deflection....");


            List<NodeResultData> lst_nrd = new List<NodeResultData>();

            iApp.SetProgressValue(10, 100);

            lst_nrd.Add(Minor_Analysis.TotalLoad_Analysis.Node_Displacements.Get_Max_Deflection());
            iApp.SetProgressValue(20, 100);

            lst_nrd.Add(Minor_Analysis.LiveLoad_1_Analysis.Node_Displacements.Get_Max_Deflection());
            iApp.SetProgressValue(30, 100);
            lst_nrd.Add(Minor_Analysis.LiveLoad_2_Analysis.Node_Displacements.Get_Max_Deflection());
            iApp.SetProgressValue(40, 100);
            lst_nrd.Add(Minor_Analysis.LiveLoad_3_Analysis.Node_Displacements.Get_Max_Deflection());
            lst_nrd.Add(Minor_Analysis.LiveLoad_4_Analysis.Node_Displacements.Get_Max_Deflection());
            iApp.SetProgressValue(50, 100);
            lst_nrd.Add(Minor_Analysis.LiveLoad_5_Analysis.Node_Displacements.Get_Max_Deflection());
            lst_nrd.Add(Minor_Analysis.LiveLoad_6_Analysis.Node_Displacements.Get_Max_Deflection());
            iApp.SetProgressValue(60, 100);

            //lst_nrd.Add(Long_Girder_Analysis.DeadLoad_Analysis.Node_Displacements.Get_Max_Deflection());


            int max_indx = 0;
            double max_def, allow_def;


            max_def = 0;
            allow_def = (L / 800.0);

            for (i = 0; i < lst_nrd.Count; i++)
            {
                if (max_def < Math.Abs(lst_nrd[i].Max_Translation))
                {
                    max_def = Math.Abs(lst_nrd[i].Max_Translation);
                    max_indx = i;
                }
            }



            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------"));
            Result.Add(string.Format("CHECK FOR LIVE LOAD DEFLECTION"));
            Result.Add(string.Format("---------------------------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(" MAXIMUM     NODE DISPLACEMENTS / ROTATIONS"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(" NODE     LOAD          X-            Y-            Z-             X-               Y-            Z-"));
            Result.Add(string.Format(" NUMBER   CASE    TRANSLATION    TRANSLATION    TRANSLATION     ROTATION        ROTATION      ROTATION"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(lst_nrd[max_indx].ToString()));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));


            Result.Add(string.Format("ALLOWABLE DEFLECTION = SPAN/800 M. = {0}/800 M. = {1} M. ", L, allow_def));
            Result.Add(string.Format(""));
            if (max_def > allow_def)
                Result.Add(string.Format("MAXIMUM  VERTICAL DEFLECTION = {0:f6} M. > {1:f6} M. ", max_def, allow_def));
            else
                Result.Add(string.Format("MAXIMUM  VERTICAL DEFLECTION = {0:f6} M. < {1:f6} M. ", max_def, allow_def));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------"));
            Result.Add(string.Format("CHECK FOR DEAD LOAD DEFLECTION LEFT SIDE OUTER GIRDER"));
            Result.Add(string.Format("---------------------------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(" MAXIMUM     NODE DISPLACEMENTS / ROTATIONS"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(" NODE     LOAD          X-            Y-            Z-             X-               Y-            Z-"));
            Result.Add(string.Format(" NUMBER   CASE    TRANSLATION    TRANSLATION    TRANSLATION     ROTATION        ROTATION      ROTATION"));
            Result.Add(string.Format(""));
            lst_nrd.Clear();
            for (i = 0; i < outer_joints.Count; i++)
            {
                lst_nrd.Add(Minor_Analysis.DeadLoad_Analysis.Node_Displacements.Get_Node_Deflection(outer_joints[i]));
                Result.Add(string.Format(lst_nrd[i].ToString()));
            }
            iApp.SetProgressValue(70, 100);

            Result.Add(string.Format(""));
            Result.Add(string.Format("ALLOWABLE DEFLECTION = SPAN/800 M. = {0}/800 M. = {1} M. ", L, allow_def));
            Result.Add(string.Format(""));
            Result.Add(string.Format("MAXIMUM NODE DISPLACEMENTS FOR LEFT SIDE OUTER LONG GIRDER"));
            Result.Add(string.Format("----------------------------------------------------------"));
            Result.Add(string.Format(""));
            for (i = 0; i < lst_nrd.Count; i++)
            {
                if (lst_nrd[i].Max_Translation < allow_def)
                    Result.Add(string.Format("MAXIMUM  VERTICAL DISPLACEMENT AT NODE {0}  = {1:f6} M. < {2:f6} M.    OK.", lst_nrd[i].NodeNo, lst_nrd[i].Max_Translation, allow_def));
                else
                    Result.Add(string.Format("MAXIMUM  VERTICAL DISPLACEMENT AT NODE {0}  = {1:f6} M. > {2:f6} M.    NOT OK.", lst_nrd[i].NodeNo, Math.Abs(lst_nrd[i].Max_Translation), allow_def));
            }

            iApp.SetProgressValue(80, 100);


            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("---------------------------------------------------------------------------------"));
            Result.Add(string.Format("CHECK FOR DEAD LOAD DEFLECTION RIGHT SIDE OUTER GIRDER"));
            Result.Add(string.Format("---------------------------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(" MAXIMUM     NODE DISPLACEMENTS / ROTATIONS"));
            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format(" NODE     LOAD          X-            Y-            Z-             X-               Y-            Z-"));
            Result.Add(string.Format(" NUMBER   CASE    TRANSLATION    TRANSLATION    TRANSLATION     ROTATION        ROTATION      ROTATION"));
            Result.Add(string.Format(""));
            lst_nrd.Clear();
            for (i = 0; i < outer_joints_right.Count; i++)
            {
                lst_nrd.Add(Minor_Analysis.DeadLoad_Analysis.Node_Displacements.Get_Node_Deflection(outer_joints_right[i]));
                Result.Add(string.Format(lst_nrd[i].ToString()));
            }
            iApp.SetProgressValue(90, 100);

            Result.Add(string.Format(""));
            Result.Add(string.Format("ALLOWABLE DEFLECTION = SPAN/800 M. = {0}/800 M. = {1} M. ", L, allow_def));
            Result.Add(string.Format(""));
            Result.Add(string.Format("MAXIMUM NODE DISPLACEMENTS FOR RIGHT SIDE OUTER LONG GIRDER"));
            Result.Add(string.Format("------------------------------------------------------------"));
            Result.Add(string.Format(""));
            for (i = 0; i < lst_nrd.Count; i++)
            {
                if (lst_nrd[i].Max_Translation < allow_def)
                    Result.Add(string.Format("MAXIMUM  VERTICAL DISPLACEMENT AT NODE {0}  = {1:f6} M. < {2:f6} M.    OK.", lst_nrd[i].NodeNo, lst_nrd[i].Max_Translation, allow_def));
                else
                    Result.Add(string.Format("MAXIMUM  VERTICAL DISPLACEMENT AT NODE {0}  = {1:f6} M. > {2:f6} M.    NOT OK.", lst_nrd[i].NodeNo, Math.Abs(lst_nrd[i].Max_Translation), allow_def));
            }


            iApp.SetProgressValue(100, 100);



            Result.Add(string.Format(""));
            Result.Add(string.Format(""));
            Result.Add(string.Format("The Deflection for Dead Load is to be controlled by providing Longitudinal"));
            Result.Add(string.Format("Camber along the length of the Main Girder between end to end supports."));






            iApp.Progress_OFF();




            Result.Add(string.Format(""));
            rtb_ana_result.Lines = Result.ToArray();

            File.WriteAllLines(File_Long_Girder_Results, Result.ToArray());






            //Long_Girder_Analysis.DeadLoad_Analysis.Node_Displacements

            iApp.RunExe(File_Long_Girder_Results);
            Result.Add(string.Format(""));
        }

        void Show_Deckslab_Moment_Shear()
        {
            MemberCollection mc = new MemberCollection(Deck_Analysis.LiveLoad_2_Analysis.Analysis.Members);

            MemberCollection sort_membs = new MemberCollection();

            JointNodeCollection jn_col = Deck_Analysis.LiveLoad_2_Analysis.Analysis.Joints;

            #region Deck Model

            double L = Deck_Analysis.Length;
            double W = Deck_Analysis.WidthBridge;
            double val = L / 2;
            int i = 0;

            Deck_Analysis.Effective_Depth = L / 16.0; ;


            List<int> _support_inn_joints = new List<int>();
            List<int> _deff_inn_joints = new List<int>();
            List<int> _L8_inn_joints = new List<int>();
            List<int> _L4_inn_joints = new List<int>();
            List<int> _3L8_inn_joints = new List<int>();
            List<int> _L2_inn_joints = new List<int>();



            List<int> _3L16_inn_joints = new List<int>();
            List<int> _5L16_inn_joints = new List<int>();
            List<int> _7L16_inn_joints = new List<int>();



            List<int> _support_out_joints = new List<int>();
            List<int> _deff_out_joints = new List<int>();
            List<int> _L8_out_joints = new List<int>();
            List<int> _L4_out_joints = new List<int>();
            List<int> _3L8_out_joints = new List<int>();
            List<int> _L2_out_joints = new List<int>();


            List<int> _3L16_out_joints = new List<int>();
            List<int> _5L16_out_joints = new List<int>();
            List<int> _7L16_out_joints = new List<int>();



            //List<int> _L4_out_joints = new List<int>();
            //List<int> _deff_out_joints = new List<int>();

            List<int> _cross_joints = new List<int>();


            List<double> _X_joints = new List<double>();
            List<double> _Z_joints = new List<double>();

            for (i = 0; i < jn_col.Count; i++)
            {
                if (_X_joints.Contains(jn_col[i].X) == false) _X_joints.Add(jn_col[i].X);
                if (_Z_joints.Contains(jn_col[i].Z) == false) _Z_joints.Add(jn_col[i].Z);
            }
            //val = MyList.StringToDouble(txt_Ana_eff_depth.Text, -999.0);
            val = Deff;

            List<double> _X_min = new List<double>();
            List<double> _X_max = new List<double>();
            double x_max, x_min;
            double vvv = 99999999999999999;
            for (int zc = 0; zc < _Z_joints.Count; zc++)
            {
                x_min = vvv;
                x_max = -vvv;

                for (i = 0; i < jn_col.Count; i++)
                {
                    //if (_X_joints.Contains(jn_col[i].X) == false) _X_joints.Add(jn_col[i].X);
                    //if (_Z_joints.Contains(jn_col[i].Z) == false) _Z_joints.Add(jn_col[i].Z);

                    if (_Z_joints[zc] == jn_col[i].Z)
                    {
                        if (x_min > jn_col[i].X)
                            x_min = jn_col[i].X;
                        if (x_max < jn_col[i].X)
                            x_max = jn_col[i].X;
                    }
                }
                if (x_max != -vvv)
                    _X_max.Add(x_max);
                if (x_min != vvv)
                    _X_min.Add(x_min);
            }
            #endregion Deck Model

            Deck_Analysis.TotalLoad_Analysis = Deck_Analysis.LiveLoad_2_Analysis;

            val = Deck_Analysis.LiveLoad_2_Analysis.Analysis.Effective_Depth;
           
            double cant_wi_left = 0.0;
            double cant_wi_right = 0.0;
        
            #region Find Joints
            for (i = 0; i < jn_col.Count; i++)
            {
                try
                {
                    if ((jn_col[i].Z >= cant_wi_left && jn_col[i].Z <= (W - cant_wi_right)) == false) continue;
                    x_min = _X_min[_Z_joints.IndexOf(jn_col[i].Z)];

                    if ((jn_col[i].X.ToString("0.0") == ((L / 2.0) + x_min).ToString("0.0")))
                    {
                        if (jn_col[i].Z >= cant_wi_left && jn_col[i].Z <= (W - cant_wi_right))
                            _L2_inn_joints.Add(jn_col[i].NodeNo);
                    }

                    if (jn_col[i].X.ToString("0.0") == ((3 * L / 8.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _3L8_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (3 * L / 8.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _3L8_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if (jn_col[i].X.ToString("0.0") == ((L / 8.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _L8_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (L / 8.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _L8_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if (jn_col[i].X.ToString("0.0") == ((L / 4.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _L4_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (L / 4.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _L4_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if ((jn_col[i].X.ToString("0.0") == (Deck_Analysis.Effective_Depth + x_min).ToString("0.0")))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _deff_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if ((jn_col[i].X.ToString("0.0") == (L - Deck_Analysis.Effective_Depth + x_min).ToString("0.0")))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _deff_out_joints.Add(jn_col[i].NodeNo);
                    }

                    if (jn_col[i].X.ToString("0.0") == (x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _support_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == (L + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _support_out_joints.Add(jn_col[i].NodeNo);
                    }

                    #region 3L/16


                    if (jn_col[i].X.ToString("0.0") == ((3 * L / 16.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _3L16_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (3 * L / 16.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _3L16_out_joints.Add(jn_col[i].NodeNo);
                    }


                    #endregion 3L/16



                    #region 5L/16


                    if (jn_col[i].X.ToString("0.0") == ((5 * L / 16.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _5L16_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (5 * L / 16.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _5L16_out_joints.Add(jn_col[i].NodeNo);
                    }


                    #endregion 3L/16



                    #region 7L/16


                    if (jn_col[i].X.ToString("0.0") == ((7 * L / 16.0) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z >= cant_wi_left)
                            _7L16_inn_joints.Add(jn_col[i].NodeNo);
                    }
                    if (jn_col[i].X.ToString("0.0") == ((L - (7 * L / 16.0)) + x_min).ToString("0.0"))
                    {
                        if (jn_col[i].Z <= (W - cant_wi_right))
                            _7L16_out_joints.Add(jn_col[i].NodeNo);
                    }


                    #endregion 3L/16



                }
                catch (Exception ex) { MessageBox.Show(this, ""); }
            }

            #endregion Find Joints

            Result.Clear();
            Result.Add("");
            Result.Add("");
            //Result.Add("Analysis Result of RCC T-BEAM Bridge");
            Result.Add("");


            #region Maximum Hogging

            List<double> Max_Hog_LL_1 = new List<double>();
            List<double> Max_Hog_LL_2 = new List<double>();
            List<double> Max_Hog_LL_3 = new List<double>();
            List<double> Max_Hog_LL_4 = new List<double>();
            List<double> Max_Hog_LL_5 = new List<double>();
            List<double> Max_Hog_LL_6 = new List<double>();


            List<double> Max_Hog_LL_7 = new List<double>();



            MaxForce f = null;

            List<int> tmp_jts = new List<int>();

            #region Live Load SHEAR Force

            #region Maximum Hogging Live Load 1

            Max_Hog_LL_1.Clear();

            //tmp_jts.Clear();
            //tmp_jts.AddRange(_support_inn_joints.ToArray());
            //f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_1.Add(val);
            

            tmp_jts.Clear();
            //tmp_jts.Add(_deff_inn_joints[1]);
            tmp_jts.Add(_deff_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_1.Add(val);


            tmp_jts.Clear();
            //tmp_jts.Add(_L8_inn_joints[1]);
            tmp_jts.Add(_L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_1.Add(val);


            tmp_jts.Clear();
            //tmp_jts.Add(_3L16_inn_joints[1]);
            tmp_jts.Add(_3L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_1.Add(val);


            tmp_jts.Clear();
            //tmp_jts.Add(_L4_inn_joints[1]);
            tmp_jts.Add(_L4_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_1.Add(val);


            tmp_jts.Clear();
            //tmp_jts.Add(_5L16_inn_joints[1]);
            tmp_jts.Add(_5L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_1.Add(val);

            tmp_jts.Clear();
            //tmp_jts.Add(_3L8_inn_joints[1]);
            tmp_jts.Add(_3L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_1.Add(val);


            tmp_jts.Clear();
            //tmp_jts.Add(_7L16_inn_joints[1]);
            tmp_jts.Add(_7L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_1.Add(val);

            tmp_jts.Clear();
            //tmp_jts.Add(_L2_inn_joints[1]);
            tmp_jts.Add(_L2_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_1.Add(val);

            tmp_jts.Clear();
            //tmp_jts.Add(_7L16_out_joints[1]);
            tmp_jts.Add(_7L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_1.Add(val);


            tmp_jts.Clear();
            //tmp_jts.AddRange(_3L8_out_joints.ToArray());
            tmp_jts.Add(_3L8_out_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_1.Add(val);


            tmp_jts.Clear();
            //tmp_jts.AddRange(_5L16_out_joints.ToArray());
            tmp_jts.Add(_5L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_1.Add(val);

            tmp_jts.Clear();
            //tmp_jts.Add(_L4_out_joints[1]);
            tmp_jts.Add(_L4_out_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_1.Add(val);


            tmp_jts.Clear();
            //tmp_jts.Add(_3L16_out_joints[1]);
            tmp_jts.Add(_3L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_1.Add(val);

            tmp_jts.Clear();
            //tmp_jts.Add(_L8_out_joints[1]);
            tmp_jts.Add(_L8_out_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_1.Add(val);

            tmp_jts.Clear();
            //tmp_jts.Add(_deff_out_joints[1]);
            tmp_jts.Add(_deff_out_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_1.Add(val);


            tmp_jts.Clear();
            //tmp_jts.Add(_support_out_joints[1]);
            tmp_jts.Add(_support_out_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_1.Add(val);


            #endregion Maximum Hogging LL 1

            #region Maximum Hogging Live Load 2

            Max_Hog_LL_2.Clear();

            //tmp_jts.Clear();
            //tmp_jts.AddRange(_support_inn_joints.ToArray());
            //f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_2.Add(val);


            //tmp_jts.Clear();
            //tmp_jts.Add(_support_out_joints[1]);
            //f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_2.Add(val);



            tmp_jts.Clear();
            tmp_jts.Add(_deff_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L4_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_5L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_2.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_3L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_7L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_2.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L2_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_2.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_7L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_2.Add(val);
            

            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_5L16_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_2.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L4_out_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_2.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L8_out_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_2.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_deff_out_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_support_out_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_2.Add(val);


            #endregion Maximum Hogging LL 2


            Max_Hog_LL_3.Clear();
            #region Maximum Hogging Live Load 1

            //tmp_jts.Clear();
            //tmp_jts.AddRange(_support_inn_joints.ToArray());
            //f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_3.Add(val);


            //tmp_jts.Clear();
            //tmp_jts.Add(_support_out_joints[1]);
            //f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_3.Add(val);



            tmp_jts.Clear();
            tmp_jts.Add(_deff_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L4_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_5L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_3.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_3L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_7L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_3.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L2_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_3.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_7L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_5L16_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_3.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L4_out_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_3.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L8_out_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_3.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_deff_out_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_3.Add(val);



            tmp_jts.Clear();
            tmp_jts.Add(_support_out_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_3.Add(val);



            #endregion Maximum Hogging LL 1

            #region Maximum Hogging Live Load 4

            Max_Hog_LL_4.Clear();

            //tmp_jts.Clear();
            //tmp_jts.AddRange(_support_inn_joints.ToArray());
            //f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_4.Add(val);


            //tmp_jts.Clear();
            //tmp_jts.Add(_support_out_joints[1]);
            //f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_4.Add(val);



            tmp_jts.Clear();
            tmp_jts.Add(_deff_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L4_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_5L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_4.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_3L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_7L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_4.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L2_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_4.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_7L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_5L16_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_4.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L4_out_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_4.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L8_out_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_4.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_deff_out_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_4.Add(val);



            tmp_jts.Clear();
            tmp_jts.Add(_support_out_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_4.Add(val);



            #endregion Maximum Hogging LL 1

            #region Maximum Hogging Live Load 5

            Max_Hog_LL_5.Clear();

            //tmp_jts.Clear();
            //tmp_jts.AddRange(_support_inn_joints.ToArray());
            //f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_5.Add(val);


            //tmp_jts.Clear();
            //tmp_jts.Add(_support_out_joints[1]);
            //f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_5.Add(val);



            tmp_jts.Clear();
            tmp_jts.Add(_deff_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L4_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_5L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_5.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_3L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_7L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_5.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L2_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_5.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_7L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_5L16_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_5.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L4_out_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_5.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L8_out_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_5.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_deff_out_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_support_out_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_5.Add(val);


            #endregion Maximum Hogging LL 5

            #region Maximum Hogging Live Load 6

            Max_Hog_LL_6.Clear();

            //tmp_jts.Clear();
            //tmp_jts.AddRange(_support_inn_joints.ToArray());
            //f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_6.Add(val);


            //tmp_jts.Clear();
            //tmp_jts.Add(_support_out_joints[1]);
            //f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_6.Add(val);



            tmp_jts.Clear();
            tmp_jts.Add(_deff_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L4_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_5L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_6.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_3L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_7L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_6.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L2_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_6.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_7L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_5L16_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_6.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L4_out_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_6.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L8_out_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_6.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_deff_out_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_6.Add(val);



            tmp_jts.Clear();
            tmp_jts.Add(_support_out_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Hogging(tmp_jts, true);
            val = f.Force;
            Max_Hog_LL_6.Add(val);



            #endregion Maximum Hogging LL 6

            #endregion Live Load SHEAR Force

            MyList.Array_Multiply_With(ref Max_Hog_LL_1, 10.0);
            MyList.Array_Multiply_With(ref Max_Hog_LL_2, 10.0);
            MyList.Array_Multiply_With(ref Max_Hog_LL_3, 10.0);
            MyList.Array_Multiply_With(ref Max_Hog_LL_4, 10.0);
            MyList.Array_Multiply_With(ref Max_Hog_LL_5, 10.0);
            MyList.Array_Multiply_With(ref Max_Hog_LL_6, 10.0);


            #endregion Maximum Hogging

            #region Maximum Sagging

            List<double> Max_Sag_LL_1 = new List<double>();
            List<double> Max_Sag_LL_2 = new List<double>();
            List<double> Max_Sag_LL_3 = new List<double>();
            List<double> Max_Sag_LL_4 = new List<double>();
            List<double> Max_Sag_LL_5 = new List<double>();
            List<double> Max_Sag_LL_6 = new List<double>();


            List<double> Max_Sag_LL_7 = new List<double>();




            #region Live Load SHEAR Force


            #region Maximum Hogging Live Load 1

            Max_Sag_LL_1.Clear();

            //tmp_jts.Clear();
            //tmp_jts.AddRange(_support_inn_joints.ToArray());
            //f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_1.Add(val);



            tmp_jts.Clear();
            tmp_jts.Add(_deff_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_1.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_1.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_1.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L4_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_1.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_5L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_1.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_3L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_1.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_7L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_1.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L2_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_1.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_7L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_1.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_1.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_5L16_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_1.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L4_out_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_1.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_1.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L8_out_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_1.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_deff_out_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_1.Add(val);



            tmp_jts.Clear();
            tmp_jts.Add(_support_out_joints[1]);
            f = Deck_Analysis.LiveLoad_1_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_1.Add(val);

            #endregion Maximum Hogging LL 1


            #region Maximum Hogging Live Load 2

            Max_Sag_LL_2.Clear();

            //tmp_jts.Clear();
            //tmp_jts.AddRange(_support_inn_joints.ToArray());
            //f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_2.Add(val);


            //tmp_jts.Clear();
            //tmp_jts.Add(_support_out_joints[1]);
            //f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_2.Add(val);



            tmp_jts.Clear();
            tmp_jts.Add(_deff_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L4_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_5L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_2.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_3L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_7L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_2.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L2_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_2.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_7L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_5L16_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_2.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L4_out_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_2.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_2.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L8_out_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_2.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_deff_out_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_2.Add(val);




            tmp_jts.Clear();
            tmp_jts.Add(_support_out_joints[1]);
            f = Deck_Analysis.LiveLoad_2_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_2.Add(val);



            #endregion Maximum Hogging LL 2


            #region Maximum Hogging Live Load 1

            Max_Sag_LL_3.Clear();

            //tmp_jts.Clear();
            //tmp_jts.AddRange(_support_inn_joints.ToArray());
            //f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_3.Add(val);


            //tmp_jts.Clear();
            //tmp_jts.Add(_support_out_joints[1]);
            //f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_3.Add(val);



            tmp_jts.Clear();
            tmp_jts.Add(_deff_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L4_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_5L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_3.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_3L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_7L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_3.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L2_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_3.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_7L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_5L16_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_3.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L4_out_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_3.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_3.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L8_out_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_3.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_deff_out_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_3.Add(val);





            tmp_jts.Clear();
            tmp_jts.Add(_support_out_joints[1]);
            f = Deck_Analysis.LiveLoad_3_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_3.Add(val);



            #endregion Maximum Hogging LL 1


            #region Maximum Hogging Live Load 4

            Max_Sag_LL_4.Clear();

            //tmp_jts.Clear();
            //tmp_jts.AddRange(_support_inn_joints.ToArray());
            //f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_4.Add(val);


            //tmp_jts.Clear();
            //tmp_jts.Add(_support_out_joints[1]);
            //f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_4.Add(val);



            tmp_jts.Clear();
            tmp_jts.Add(_deff_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L4_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_5L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_4.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_3L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_7L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_4.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L2_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_4.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_7L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_5L16_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_4.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L4_out_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_4.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_4.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L8_out_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_4.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_deff_out_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_4.Add(val);




            tmp_jts.Clear();
            tmp_jts.Add(_support_out_joints[1]);
            f = Deck_Analysis.LiveLoad_4_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_4.Add(val);



            #endregion Maximum Hogging LL 1


            #region Maximum Hogging Live Load 5

            Max_Sag_LL_5.Clear();

            //tmp_jts.Clear();
            //tmp_jts.AddRange(_support_inn_joints.ToArray());
            //f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_5.Add(val);


            //tmp_jts.Clear();
            //tmp_jts.Add(_support_out_joints[1]);
            //f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_5.Add(val);



            tmp_jts.Clear();
            tmp_jts.Add(_deff_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L4_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_5L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_5.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_3L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_7L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_5.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L2_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_5.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_7L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_5L16_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_5.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L4_out_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_5.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_5.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L8_out_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_5.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_deff_out_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_5.Add(val);




            tmp_jts.Clear();
            tmp_jts.Add(_support_out_joints[1]);
            f = Deck_Analysis.LiveLoad_5_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_5.Add(val);



            #endregion Maximum Hogging LL 5



            #region Maximum Hogging Live Load 6

            Max_Sag_LL_6.Clear();

            //tmp_jts.Clear();
            //tmp_jts.AddRange(_support_inn_joints.ToArray());
            //f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_6.Add(val);


            //tmp_jts.Clear();
            //tmp_jts.Add(_support_out_joints[1]);
            //f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            //val = f.Force;
            //SF_LL_6.Add(val);



            tmp_jts.Clear();
            tmp_jts.Add(_deff_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_L4_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_5L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_6.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_3L8_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_7L16_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_6.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L2_inn_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_6.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_7L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_3L8_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.AddRange(_5L16_out_joints.ToArray());
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_6.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L4_out_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_6.Add(val);


            tmp_jts.Clear();
            tmp_jts.Add(_3L16_out_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_6.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_L8_out_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_6.Add(val);

            tmp_jts.Clear();
            tmp_jts.Add(_deff_out_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_6.Add(val);




            tmp_jts.Clear();
            tmp_jts.Add(_support_out_joints[1]);
            f = Deck_Analysis.LiveLoad_6_Analysis.GetJoint_Max_Sagging(tmp_jts, true);
            val = f.Force;
            Max_Sag_LL_6.Add(val);



            #endregion Maximum Hogging LL 6



            #endregion Live Load SHEAR Force

            MyList.Array_Multiply_With(ref Max_Sag_LL_1, 10.0);
            MyList.Array_Multiply_With(ref Max_Sag_LL_2, 10.0);
            MyList.Array_Multiply_With(ref Max_Sag_LL_3, 10.0);
            MyList.Array_Multiply_With(ref Max_Sag_LL_4, 10.0);
            MyList.Array_Multiply_With(ref Max_Sag_LL_5, 10.0);
            MyList.Array_Multiply_With(ref Max_Sag_LL_6, 10.0);


            #endregion Maximum Hogging





            #region DL + SIDL + FPLL

            List<double> Max_Moment_DL = new List<double>();
            List<double> Max_Moment_SIDL_1 = new List<double>();
            List<double> Max_Moment_SIDL_2 = new List<double>();
            List<double> Max_Moment_FPLL = new List<double>();

            #region _deff_inn_joints
            tmp_jts.Clear();


            tmp_jts.Add(_deff_inn_joints[1]);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 1);
            val = f.Force;
            Max_Moment_DL.Add(val);


            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 2);
            val = f.Force;
            Max_Moment_SIDL_1.Add(val);



            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 3);
            val = f.Force;
            Max_Moment_SIDL_2.Add(val);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 4);
            val = f.Force;
            Max_Moment_FPLL.Add(val);
            #endregion _deff_inn_joints

            #region _L8_inn_joints
            tmp_jts.Clear();


            tmp_jts.Add(_L8_inn_joints[1]);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 1);
            val = f.Force;
            Max_Moment_DL.Add(val);


            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 2);
            val = f.Force;
            Max_Moment_SIDL_1.Add(val);



            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 3);
            val = f.Force;
            Max_Moment_SIDL_2.Add(val);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 4);
            val = f.Force;
            Max_Moment_FPLL.Add(val);
            #endregion _L8_inn_joints

            #region _3L16_inn_joints
            tmp_jts.Clear();


            tmp_jts.Add(_3L16_inn_joints[1]);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 1);
            val = f.Force;
            Max_Moment_DL.Add(val);


            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 2);
            val = f.Force;
            Max_Moment_SIDL_1.Add(val);



            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 3);
            val = f.Force;
            Max_Moment_SIDL_2.Add(val);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 4);
            val = f.Force;
            Max_Moment_FPLL.Add(val);
            #endregion _deff_inn_joints

            #region _L4_inn_joints
            tmp_jts.Clear();


            tmp_jts.Add(_L4_inn_joints[1]);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 1);
            val = f.Force;
            Max_Moment_DL.Add(val);


            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 2);
            val = f.Force;
            Max_Moment_SIDL_1.Add(val);



            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 3);
            val = f.Force;
            Max_Moment_SIDL_2.Add(val);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 4);
            val = f.Force;
            Max_Moment_FPLL.Add(val);
            #endregion _L4_inn_joints


            #region _5L16_inn_joints
            tmp_jts.Clear();


            tmp_jts.Add(_5L16_inn_joints[1]);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 1);
            val = f.Force;
            Max_Moment_DL.Add(val);


            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 2);
            val = f.Force;
            Max_Moment_SIDL_1.Add(val);



            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 3);
            val = f.Force;
            Max_Moment_SIDL_2.Add(val);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 4);
            val = f.Force;
            Max_Moment_FPLL.Add(val);
            #endregion _deff_inn_joints


            #region _3L8_inn_joints
            tmp_jts.Clear();


            tmp_jts.Add(_3L8_inn_joints[1]);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 1);
            val = f.Force;
            Max_Moment_DL.Add(val);


            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 2);
            val = f.Force;
            Max_Moment_SIDL_1.Add(val);



            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 3);
            val = f.Force;
            Max_Moment_SIDL_2.Add(val);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 4);
            val = f.Force;
            Max_Moment_FPLL.Add(val);
            #endregion _3L8_inn_joints


            #region _7L16_inn_joints
            tmp_jts.Clear();


            tmp_jts.Add(_7L16_inn_joints[1]);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 1);
            val = f.Force;
            Max_Moment_DL.Add(val);


            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 2);
            val = f.Force;
            Max_Moment_SIDL_1.Add(val);



            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 3);
            val = f.Force;
            Max_Moment_SIDL_2.Add(val);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 4);
            val = f.Force;
            Max_Moment_FPLL.Add(val);
            #endregion _7L16_inn_joints


            #region _L2_inn_joints

            tmp_jts.Clear();


            tmp_jts.Add(_L2_inn_joints[1]);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 1);
            val = f.Force;
            Max_Moment_DL.Add(val);


            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 2);
            val = f.Force;
            Max_Moment_SIDL_1.Add(val);



            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 3);
            val = f.Force;
            Max_Moment_SIDL_2.Add(val);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 4);
            val = f.Force;
            Max_Moment_FPLL.Add(val);

            #endregion _L2_inn_joints


            #region _7L16_out_joints
            tmp_jts.Clear();


            tmp_jts.Add(_7L16_out_joints[1]);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 1);
            val = f.Force;
            Max_Moment_DL.Add(val);


            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 2);
            val = f.Force;
            Max_Moment_SIDL_1.Add(val);



            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 3);
            val = f.Force;
            Max_Moment_SIDL_2.Add(val);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 4);
            val = f.Force;
            Max_Moment_FPLL.Add(val);
            #endregion _7L16_out_joints


            #region _3L8_out_joints
            tmp_jts.Clear();

            tmp_jts.AddRange(_3L8_out_joints.ToArray());

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 1);
            val = f.Force;
            Max_Moment_DL.Add(val);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 2);
            val = f.Force;
            Max_Moment_SIDL_1.Add(val);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 3);
            val = f.Force;
            Max_Moment_SIDL_2.Add(val);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 4);
            val = f.Force;
            Max_Moment_FPLL.Add(val);

            #endregion _3L8_out_joints


            #region _5L16_out_joints
            tmp_jts.Clear();


            tmp_jts.AddRange(_5L16_out_joints.ToArray());

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 1);
            val = f.Force;
            Max_Moment_DL.Add(val);


            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 2);
            val = f.Force;
            Max_Moment_SIDL_1.Add(val);



            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 3);
            val = f.Force;
            Max_Moment_SIDL_2.Add(val);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 4);
            val = f.Force;
            Max_Moment_FPLL.Add(val);

            #endregion _5L16_out_joints


            #region _L4_out_joints
            tmp_jts.Clear();


            tmp_jts.Add(_L4_out_joints[1]);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 1);
            val = f.Force;
            Max_Moment_DL.Add(val);


            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 2);
            val = f.Force;
            Max_Moment_SIDL_1.Add(val);



            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 3);
            val = f.Force;
            Max_Moment_SIDL_2.Add(val);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 4);
            val = f.Force;
            Max_Moment_FPLL.Add(val);

            #endregion _L4_out_joints

            #region _3L16_out_joints
            tmp_jts.Clear();


            tmp_jts.Add(_3L16_out_joints[1]);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 1);
            val = f.Force;
            Max_Moment_DL.Add(val);


            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 2);
            val = f.Force;
            Max_Moment_SIDL_1.Add(val);



            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 3);
            val = f.Force;
            Max_Moment_SIDL_2.Add(val);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 4);
            val = f.Force;
            Max_Moment_FPLL.Add(val);

            #endregion _3L16_out_joints

            #region _L8_out_joints
            tmp_jts.Clear();


            tmp_jts.Add(_L8_out_joints[1]);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 1);
            val = f.Force;
            Max_Moment_DL.Add(val);


            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 2);
            val = f.Force;
            Max_Moment_SIDL_1.Add(val);



            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 3);
            val = f.Force;
            Max_Moment_SIDL_2.Add(val);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 4);
            val = f.Force;
            Max_Moment_FPLL.Add(val);
            #endregion _L8_out_joints

            #region _deff_out_joints
            tmp_jts.Clear();


            tmp_jts.Add(_deff_out_joints[1]);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 1);
            val = f.Force;
            Max_Moment_DL.Add(val);


            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 2);
            val = f.Force;
            Max_Moment_SIDL_1.Add(val);



            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 3);
            val = f.Force;
            Max_Moment_SIDL_2.Add(val);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 4);
            val = f.Force;
            Max_Moment_FPLL.Add(val);

            #endregion _deff_out_joints

            #region _support_out_joints
            tmp_jts.Clear();


            tmp_jts.Add(_support_out_joints[1]);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 1);
            val = f.Force;
            Max_Moment_DL.Add(val);


            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 2);
            val = f.Force;
            Max_Moment_SIDL_1.Add(val);



            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 3);
            val = f.Force;
            Max_Moment_SIDL_2.Add(val);

            f = Deck_Analysis.DeadLoad_Analysis.GetJoint_MomentForce(tmp_jts[0], true, 4);
            val = f.Force;
            Max_Moment_FPLL.Add(val);

            #endregion _support_out_joints



            MyList.Array_Multiply_With(ref Max_Moment_DL, 10.0);
            MyList.Array_Multiply_With(ref Max_Moment_SIDL_1, 10.0);
            MyList.Array_Multiply_With(ref Max_Moment_SIDL_2, 10.0);
            MyList.Array_Multiply_With(ref Max_Moment_FPLL, 10.0);

            #endregion DL + SIDL + FPLL

            val = 0.0;


            //string format = "{0,-25} {1,12:f3} {2,10:f3} {3,15:f3} {4,10:f3} {5,10:f3} {6,10:f3} {7,10:f3} {8,10:f3} {9,10:f3} {10,10:f3} {11,10:f3} {12,10:f3} {13,10:f3} {14,10:f3} {15,10:f3} {16,10:f3}";
            string format = "{0,-25} {1,12:f3} {2,10:f3} {3,15:f3} {4,10:f3} {5,10:f3} {6,10:f3} {7,10:f3} {8,10:f3} {9,10:f3} {10,10:f3} {11,10:f3} {12,10:f3} {13,10:f3} {14,10:f3} {15,10:f3}";

            Result.Add(string.Format(""));

            #region Summary 1

            Result.Add(string.Format(""));
            Result.Add(string.Format("-------------------------------------------------------------"));
            Result.Add(string.Format("RCC T GIRDER (LIMIT STATE METHOD) DECK SLAB ANALYSIS RESULTS "));
            Result.Add(string.Format("--------------------------------------------------------------"));
            Result.Add(string.Format(""));
            Result.Add(string.Format("----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- "));
            //Result.Add(string.Format("                                NODE 5     NODE 5          NODE 8     NODE 11    NODE 14    NODE 17    NODE 20    NODE 23    NODE 26   NODE 29    NODE 32    NODE 35    NODE 38    NODE 41    NODE 44    NODE 47"));
            Result.Add(string.Format("                                NODE 5     NODE 8          NODE 11    NODE 14    NODE 17    NODE 20    NODE 23    NODE 26   NODE 29    NODE 32    NODE 35    NODE 38    NODE 41    NODE 44    NODE 47"));
            Result.Add(string.Format("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------"));
            Result.Add(string.Format(""));
            int indx = 0;
            try
            {

                #region Max Dead Load

                indx = 0;
                Result.Add(string.Format(format,
                    "DL",
                    Max_Moment_DL[indx++],
                    Max_Moment_DL[indx++],
                    Max_Moment_DL[indx++],
                    Max_Moment_DL[indx++],
                    Max_Moment_DL[indx++],
                    Max_Moment_DL[indx++],
                    Max_Moment_DL[indx++],
                    Max_Moment_DL[indx++],
                    Max_Moment_DL[indx++],
                    Max_Moment_DL[indx++],
                    Max_Moment_DL[indx++],
                    Max_Moment_DL[indx++],
                    Max_Moment_DL[indx++],
                    Max_Moment_DL[indx++],
                    Max_Moment_DL[indx++],
                    Max_Moment_DL[indx++]
                    ));

                #endregion  Max Dead Load


                #region Max SIDL (Except surfacing)

                indx = 0;
                Result.Add(string.Format(format,
                    "SIDL (Except surfacing)",
                    Max_Moment_SIDL_1[indx++],
                    Max_Moment_SIDL_1[indx++],
                    Max_Moment_SIDL_1[indx++],
                    Max_Moment_SIDL_1[indx++],
                    Max_Moment_SIDL_1[indx++],
                    Max_Moment_SIDL_1[indx++],
                    Max_Moment_SIDL_1[indx++],
                    Max_Moment_SIDL_1[indx++],
                    Max_Moment_SIDL_1[indx++],
                    Max_Moment_SIDL_1[indx++],
                    Max_Moment_SIDL_1[indx++],
                    Max_Moment_SIDL_1[indx++],
                    Max_Moment_SIDL_1[indx++],
                    Max_Moment_SIDL_1[indx++],
                    Max_Moment_SIDL_1[indx++],
                    Max_Moment_SIDL_1[indx++]
                    ));

                #endregion  Max SIDL (Except surfacing)

                #region Max SIDL (Surfacing)

                indx = 0;
                Result.Add(string.Format(format,
                    "SIDL (Surfacing)",
                    Max_Moment_SIDL_2[indx++],
                    Max_Moment_SIDL_2[indx++],
                    Max_Moment_SIDL_2[indx++],
                    Max_Moment_SIDL_2[indx++],
                    Max_Moment_SIDL_2[indx++],
                    Max_Moment_SIDL_2[indx++],
                    Max_Moment_SIDL_2[indx++],
                    Max_Moment_SIDL_2[indx++],
                    Max_Moment_SIDL_2[indx++],
                    Max_Moment_SIDL_2[indx++],
                    Max_Moment_SIDL_2[indx++],
                    Max_Moment_SIDL_2[indx++],
                    Max_Moment_SIDL_2[indx++],
                    Max_Moment_SIDL_2[indx++],
                    Max_Moment_SIDL_2[indx++],
                    Max_Moment_SIDL_2[indx++]
                    ));

                #endregion  Max SIDL (Surfacing)


                #region Max FPLL

                indx = 0;
                Result.Add(string.Format(format,
                    "FPLL",
                    Max_Moment_FPLL[indx++],
                     Max_Moment_FPLL[indx++],
                     Max_Moment_FPLL[indx++],
                     Max_Moment_FPLL[indx++],
                     Max_Moment_FPLL[indx++],
                     Max_Moment_FPLL[indx++],
                     Max_Moment_FPLL[indx++],
                     Max_Moment_FPLL[indx++],
                     Max_Moment_FPLL[indx++],
                     Max_Moment_FPLL[indx++],
                     Max_Moment_FPLL[indx++],
                     Max_Moment_FPLL[indx++],
                     Max_Moment_FPLL[indx++],
                     Max_Moment_FPLL[indx++],
                     Max_Moment_FPLL[indx++],
                     Max_Moment_FPLL[indx++]
                    ));

                #endregion  Max FPLL


                #region Max LL_1

                #region Max_Hog_LL_1
                indx = 0;
                Result.Add(string.Format(format,
                    "1CLA (Max. Hog)",
                    Max_Hog_LL_1[indx++],
                    Max_Hog_LL_1[indx++],
                    Max_Hog_LL_1[indx++],
                    Max_Hog_LL_1[indx++],
                    Max_Hog_LL_1[indx++],
                    Max_Hog_LL_1[indx++],
                    Max_Hog_LL_1[indx++],
                    Max_Hog_LL_1[indx++],
                    Max_Hog_LL_1[indx++],
                    Max_Hog_LL_1[indx++],
                    Max_Hog_LL_1[indx++],
                    Max_Hog_LL_1[indx++],
                    Max_Hog_LL_1[indx++],
                    Max_Hog_LL_1[indx++],
                    Max_Hog_LL_1[indx++],
                    Max_Hog_LL_1[indx++]
                    ));
                #endregion Max_Hog_LL_1

                #region Max_Sag_LL_1
                indx = 0;
                Result.Add(string.Format(format,
                    "1CLA (Max. Sag)",
                    Max_Sag_LL_1[indx++],
                    Max_Sag_LL_1[indx++],
                    Max_Sag_LL_1[indx++],
                    Max_Sag_LL_1[indx++],
                    Max_Sag_LL_1[indx++],
                    Max_Sag_LL_1[indx++],
                    Max_Sag_LL_1[indx++],
                    Max_Sag_LL_1[indx++],
                    Max_Sag_LL_1[indx++],
                    Max_Sag_LL_1[indx++],
                    Max_Sag_LL_1[indx++],
                    Max_Sag_LL_1[indx++],
                    Max_Sag_LL_1[indx++],
                    Max_Sag_LL_1[indx++],
                    Max_Sag_LL_1[indx++],
                    Max_Sag_LL_1[indx++]
                    ));
                #endregion Max_Hog_LL_1

                #endregion Max LL1

                #region Max LL_2
                #region Max_Hog_LL_2
                indx = 0;
                Result.Add(string.Format(format,
                    "2CLA (Max. Hog)",
                    Max_Hog_LL_2[indx++],
                    Max_Hog_LL_2[indx++],
                    Max_Hog_LL_2[indx++],
                    Max_Hog_LL_2[indx++],
                    Max_Hog_LL_2[indx++],
                    Max_Hog_LL_2[indx++],
                    Max_Hog_LL_2[indx++],
                    Max_Hog_LL_2[indx++],
                    Max_Hog_LL_2[indx++],
                    Max_Hog_LL_2[indx++],
                    Max_Hog_LL_2[indx++],
                    Max_Hog_LL_2[indx++],
                    Max_Hog_LL_2[indx++],
                    Max_Hog_LL_2[indx++],
                    Max_Hog_LL_2[indx++],
                    Max_Hog_LL_2[indx++]
                    ));
                #endregion Max_Hog_LL_2

                #region Max_Sag_LL_2
                indx = 0;
                Result.Add(string.Format(format,
                    "2CLA (Max. Sag)",
                    Max_Sag_LL_2[indx++],
                    Max_Sag_LL_2[indx++],
                    Max_Sag_LL_2[indx++],
                    Max_Sag_LL_2[indx++],
                    Max_Sag_LL_2[indx++],
                    Max_Sag_LL_2[indx++],
                    Max_Sag_LL_2[indx++],
                    Max_Sag_LL_2[indx++],
                    Max_Sag_LL_2[indx++],
                    Max_Sag_LL_2[indx++],
                    Max_Sag_LL_2[indx++],
                    Max_Sag_LL_2[indx++],
                    Max_Sag_LL_2[indx++],
                    Max_Sag_LL_2[indx++],
                    Max_Sag_LL_2[indx++],
                    Max_Sag_LL_2[indx++]
                    ));
                #endregion Max_Hog_LL_2

                #endregion Max LL1


                #region Max LL_3
                #region Max_Hog_LL_3
                indx = 0;
                Result.Add(string.Format(format,
                    "1L 40T-L (Max. Hog)",
                    Max_Hog_LL_3[indx++],
                    Max_Hog_LL_3[indx++],
                    Max_Hog_LL_3[indx++],
                    Max_Hog_LL_3[indx++],
                    Max_Hog_LL_3[indx++],
                    Max_Hog_LL_3[indx++],
                    Max_Hog_LL_3[indx++],
                    Max_Hog_LL_3[indx++],
                    Max_Hog_LL_3[indx++],
                    Max_Hog_LL_3[indx++],
                    Max_Hog_LL_3[indx++],
                    Max_Hog_LL_3[indx++],
                    Max_Hog_LL_3[indx++],
                    Max_Hog_LL_3[indx++],
                    Max_Hog_LL_3[indx++],
                    Max_Hog_LL_3[indx++]
                    ));
                #endregion Max_Hog_LL_3

                #region Max_Sag_LL_3
                indx = 0;
                Result.Add(string.Format(format,
                    "1L 40T-L (Max. Sag)",
                    Max_Sag_LL_3[indx++],
                    Max_Sag_LL_3[indx++],
                    Max_Sag_LL_3[indx++],
                    Max_Sag_LL_3[indx++],
                    Max_Sag_LL_3[indx++],
                    Max_Sag_LL_3[indx++],
                    Max_Sag_LL_3[indx++],
                    Max_Sag_LL_3[indx++],
                    Max_Sag_LL_3[indx++],
                    Max_Sag_LL_3[indx++],
                    Max_Sag_LL_3[indx++],
                    Max_Sag_LL_3[indx++],
                    Max_Sag_LL_3[indx++],
                    Max_Sag_LL_3[indx++],
                    Max_Sag_LL_3[indx++],
                    Max_Sag_LL_3[indx++]
                    ));
                #endregion Max_Hog_LL_3

                #endregion Max LL1

                #region Max LL_4
                #region Max_Hog_LL_4
                indx = 0;
                Result.Add(string.Format(format,
                    "1L 40T-M (Max. Hog)",
                    Max_Hog_LL_4[indx++],
                    Max_Hog_LL_4[indx++],
                    Max_Hog_LL_4[indx++],
                    Max_Hog_LL_4[indx++],
                    Max_Hog_LL_4[indx++],
                    Max_Hog_LL_4[indx++],
                    Max_Hog_LL_4[indx++],
                    Max_Hog_LL_4[indx++],
                    Max_Hog_LL_4[indx++],
                    Max_Hog_LL_4[indx++],
                    Max_Hog_LL_4[indx++],
                    Max_Hog_LL_4[indx++],
                    Max_Hog_LL_4[indx++],
                    Max_Hog_LL_4[indx++],
                    Max_Hog_LL_4[indx++],
                    Max_Hog_LL_4[indx++]
                    ));
                #endregion Max_Hog_LL_4

                #region Max_Sag_LL_4
                indx = 0;
                Result.Add(string.Format(format,
                    "1L 40T-M (Max. Sag)",
                    Max_Sag_LL_4[indx++],
                    Max_Sag_LL_4[indx++],
                    Max_Sag_LL_4[indx++],
                    Max_Sag_LL_4[indx++],
                    Max_Sag_LL_4[indx++],
                    Max_Sag_LL_4[indx++],
                    Max_Sag_LL_4[indx++],
                    Max_Sag_LL_4[indx++],
                    Max_Sag_LL_4[indx++],
                    Max_Sag_LL_4[indx++],
                    Max_Sag_LL_4[indx++],
                    Max_Sag_LL_4[indx++],
                    Max_Sag_LL_4[indx++],
                    Max_Sag_LL_4[indx++],
                    Max_Sag_LL_4[indx++],
                    Max_Sag_LL_4[indx++]
                    ));
                #endregion Max_Hog_LL_4

                #endregion Max LL1

                #region Max LL_5
                #region Max_Hog_LL_5
                indx = 0;
                Result.Add(string.Format(format,
                    "1L 70R TR (Max. Hog)",
                    Max_Hog_LL_5[indx++],
                    Max_Hog_LL_5[indx++],
                    Max_Hog_LL_5[indx++],
                    Max_Hog_LL_5[indx++],
                    Max_Hog_LL_5[indx++],
                    Max_Hog_LL_5[indx++],
                    Max_Hog_LL_5[indx++],
                    Max_Hog_LL_5[indx++],
                    Max_Hog_LL_5[indx++],
                    Max_Hog_LL_5[indx++],
                    Max_Hog_LL_5[indx++],
                    Max_Hog_LL_5[indx++],
                    Max_Hog_LL_5[indx++],
                    Max_Hog_LL_5[indx++],
                    Max_Hog_LL_5[indx++],
                    Max_Hog_LL_5[indx++]
                    ));
                #endregion Max_Hog_LL_5

                #region Max_Sag_LL_5
                indx = 0;
                Result.Add(string.Format(format,
                    "1L 70R TR (Max. Sag)",
                    Max_Sag_LL_5[indx++],
                    Max_Sag_LL_5[indx++],
                    Max_Sag_LL_5[indx++],
                    Max_Sag_LL_5[indx++],
                    Max_Sag_LL_5[indx++],
                    Max_Sag_LL_5[indx++],
                    Max_Sag_LL_5[indx++],
                    Max_Sag_LL_5[indx++],
                    Max_Sag_LL_5[indx++],
                    Max_Sag_LL_5[indx++],
                    Max_Sag_LL_5[indx++],
                    Max_Sag_LL_5[indx++],
                    Max_Sag_LL_5[indx++],
                    Max_Sag_LL_5[indx++],
                    Max_Sag_LL_5[indx++],
                    Max_Sag_LL_5[indx++]
                    ));
                #endregion Max_Hog_LL_5

                #endregion Max LL1

                #region Max LL_6
                #region Max_Hog_LL_6
                indx = 0;
                Result.Add(string.Format(format,
                    "1L70RW+1LCA(Max Hog)",
                    Max_Hog_LL_6[indx++],
                    Max_Hog_LL_6[indx++],
                    Max_Hog_LL_6[indx++],
                    Max_Hog_LL_6[indx++],
                    Max_Hog_LL_6[indx++],
                    Max_Hog_LL_6[indx++],
                    Max_Hog_LL_6[indx++],
                    Max_Hog_LL_6[indx++],
                    Max_Hog_LL_6[indx++],
                    Max_Hog_LL_6[indx++],
                    Max_Hog_LL_6[indx++],
                    Max_Hog_LL_6[indx++],
                    Max_Hog_LL_6[indx++],
                    Max_Hog_LL_6[indx++],
                    Max_Hog_LL_6[indx++],
                    Max_Hog_LL_6[indx++]
                    ));
                #endregion Max_Hog_LL_6

                #region Max_Sag_LL_6
                indx = 0;
                Result.Add(string.Format(format,
                    "1L70RW+1LCA(Max Sag)",
                    Max_Sag_LL_6[indx++],
                    Max_Sag_LL_6[indx++],
                    Max_Sag_LL_6[indx++],
                    Max_Sag_LL_6[indx++],
                    Max_Sag_LL_6[indx++],
                    Max_Sag_LL_6[indx++],
                    Max_Sag_LL_6[indx++],
                    Max_Sag_LL_6[indx++],
                    Max_Sag_LL_6[indx++],
                    Max_Sag_LL_6[indx++],
                    Max_Sag_LL_6[indx++],
                    Max_Sag_LL_6[indx++],
                    Max_Sag_LL_6[indx++],
                    Max_Sag_LL_6[indx++],
                    Max_Sag_LL_6[indx++],
                    Max_Sag_LL_6[indx++]
                    ));
                #endregion Max_Hog_LL_6

                #endregion Max LL1



                List<double> temp = new List<double>();
                for (i = 0; i < Max_Hog_LL_1.Count; i++)
                {
                    temp.Add(Max_Hog_LL_1[i]);
                    temp.Add(Max_Hog_LL_2[i]);
                    temp.Add(Max_Hog_LL_3[i]);
                    temp.Add(Max_Hog_LL_4[i]);
                    temp.Add(Max_Hog_LL_5[i]);
                    temp.Add(Max_Hog_LL_6[i]);

                    temp.Sort();
                    temp.Reverse();

                    Max_Hog_LL_7.Add(temp[0]);

                    temp.Clear();

                    temp.Add(Max_Sag_LL_1[i]);
                    temp.Add(Max_Sag_LL_2[i]);
                    temp.Add(Max_Sag_LL_3[i]);
                    temp.Add(Max_Sag_LL_4[i]);
                    temp.Add(Max_Sag_LL_5[i]);
                    temp.Add(Max_Sag_LL_6[i]);

                    temp.Sort();
                    //temp.Reverse();

                    Max_Sag_LL_7.Add(temp[0]);
                }



                #region Max LL_7
                #region Max_Hog_LL_7
                indx = 0;
                Result.Add(string.Format(format,
                    "LL (Max Hog)",
                    Max_Hog_LL_7[indx++],
                    Max_Hog_LL_7[indx++],
                    Max_Hog_LL_7[indx++],
                    Max_Hog_LL_7[indx++],
                    Max_Hog_LL_7[indx++],
                    Max_Hog_LL_7[indx++],
                    Max_Hog_LL_7[indx++],
                    Max_Hog_LL_7[indx++],
                    Max_Hog_LL_7[indx++],
                    Max_Hog_LL_7[indx++],
                    Max_Hog_LL_7[indx++],
                    Max_Hog_LL_7[indx++],
                    Max_Hog_LL_7[indx++],
                    Max_Hog_LL_7[indx++],
                    Max_Hog_LL_7[indx++],
                    Max_Hog_LL_7[indx++]
                    ));
                #endregion Max_Hog_LL_7

                #region Max_Sag_LL_7
                indx = 0;
                Result.Add(string.Format(format,
                    "LL (Max Sag)",
                    Max_Sag_LL_7[indx++],
                    Max_Sag_LL_7[indx++],
                    Max_Sag_LL_7[indx++],
                    Max_Sag_LL_7[indx++],
                    Max_Sag_LL_7[indx++],
                    Max_Sag_LL_7[indx++],
                    Max_Sag_LL_7[indx++],
                    Max_Sag_LL_7[indx++],
                    Max_Sag_LL_7[indx++],
                    Max_Sag_LL_7[indx++],
                    Max_Sag_LL_7[indx++],
                    Max_Sag_LL_7[indx++],
                    Max_Sag_LL_7[indx++],
                    Max_Sag_LL_7[indx++],
                    Max_Sag_LL_7[indx++],
                    Max_Sag_LL_7[indx++]
                    ));
                #endregion Max_Hog_LL_7

                #endregion Max LL1


            }
            catch (Exception exxx) { }
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- "));
            Result.Add(string.Format("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- "));
            Result.Add(string.Format(""));

            #endregion Summary 1
 



            Result.Add(string.Format(""));
            //rtb_ana_result.Lines = Result.ToArray();

            File.WriteAllLines(File_DeckSlab_Results, Result.ToArray());

            iApp.RunExe(File_DeckSlab_Results);
            Result.Add(string.Format(""));
        }

        public string File_Long_Girder_Results
        {
            get
            {
                return Path.Combine(user_path, "LONG_GIRDER_ANALYSIS_RESULT.TXT");
            }
        }
        public string File_DeckSlab_Results
        {
            get
            {
                return Path.Combine(user_path, "DECKSLAB_ANALYSIS_RESULT.TXT");
            }
        }

        public void Button_Enable_Disable()
        {
            btn_view_data.Enabled = File.Exists(Minor_Analysis.TotalAnalysis_Input_File);
            btn_view_structure.Enabled = File.Exists(Minor_Analysis.TotalAnalysis_Input_File);
            btn_View_Moving_Load.Enabled = File.Exists(Minor_Analysis.Total_Analysis_Report);
            btn_view_report.Enabled = File.Exists(Minor_Analysis.Total_Analysis_Report);
            btn_process_analysis.Enabled = File.Exists(Minor_Analysis.TotalAnalysis_Input_File);

            btn_RCC_Pier_Report.Enabled = File.Exists(rcc_pier.rep_file_name);
            //btn_cnt_Report.Enabled = File.Exists(Abut.rep_file_name);

            //if (Deck_Analysis != null)
            //{
            //    btn_Deck_Analysis.Enabled = File.Exists(Deck_Analysis.Input_File);
            //    btn_LS_deck_ws.Enabled = File.Exists(File_DeckSlab_Results);
            //}
            //else
            //{
            //    btn_Deck_Analysis.Enabled = false;
            //    btn_LS_deck_ws.Enabled = false;
            //}
            if (Minor_Analysis != null)
            {
                btn_process_analysis.Enabled = File.Exists(Minor_Analysis.Input_File);
                btn_LS_long_ws.Enabled = File.Exists(File_Long_Girder_Results);
            }


            Deck_Buttons();
            
            btn_LS_long_rep_open.Enabled = File.Exists(Path.Combine(Worksheet_Folder, Path.GetFileName(Excel_Long_Girder)));
            btn_LS_cross_rep_open.Enabled = File.Exists(Path.Combine(Worksheet_Folder, Path.GetFileName(Excel_Cross_Girder)));
            //btn_LS_deck_rep_open.Enabled = File.Exists(Path.Combine(Worksheet_Folder, Path.GetFileName(Excel_Deckslab)));



            int c = cmb_long_open_file.SelectedIndex;
            cmb_long_open_file.SelectedIndex = -1;
            cmb_long_open_file.SelectedIndex = c;
        }




        public void Ana_OpenAnalysisFile(string file_name)
        {
            string analysis_file = "";
            analysis_file = file_name;



            string usp = Path.Combine(user_path, "Deck Slab Analysis");
            if (Directory.Exists(usp))
            {
                Deck_Analysis.Input_File = Path.Combine(usp, "INPUT_DATA.TXT");
            }
            usp = Path.Combine(user_path, "Long Girder Analysis");
            if (Directory.Exists(usp))
            {
                Minor_Analysis.Input_File = Path.Combine(usp, "INPUT_DATA.TXT");
            }


            if (File.Exists(analysis_file))
            {
                btn_view_structure.Enabled = true;
                Abut.FilePath = user_path;
                rcc_pier.FilePath = user_path;
            }
            Button_Enable_Disable();
        }

        List<string> load_list_1 = new List<string>();
        List<string> load_list_2 = new List<string>();
        List<string> load_list_3 = new List<string>();
        List<string> load_list_4 = new List<string>();
        List<string> load_list_5 = new List<string>();
        List<string> load_list_6 = new List<string>();
        List<string> load_total_7 = new List<string>();


        List<string> Ana_Get_Joints_Load(double load)
        {
            MemberCollection mc = new MemberCollection(Minor_Analysis.TotalLoad_Analysis.Analysis.Members);

            MemberCollection sort_membs = new MemberCollection();

            double z_min = double.MaxValue;
            double x = double.MaxValue;
            int indx = -1;

            int i = 0;
            int j = 0;

            List<double> list_z = new List<double>();
            List<string> list_arr = new List<string>();

            List<MemberCollection> list_mc = new List<MemberCollection>();

            double last_z = 0.0;

            while (mc.Count != 0)
            {
                indx = -1;
                for (i = 0; i < mc.Count; i++)
                {
                    if (z_min > mc[i].StartNode.Z)
                    {
                        z_min = mc[i].StartNode.Z;
                        indx = i;
                    }
                }
                if (indx != -1)
                {

                    if (!list_z.Contains(z_min))
                        list_z.Add(z_min);

                    sort_membs.Add(mc[indx]);
                    mc.Members.RemoveAt(indx);
                    z_min = double.MaxValue;
                }
            }

            last_z = -1.0;

            //Inner & Outer Long Girder
            MemberCollection outer_long = new MemberCollection();
            MemberCollection inner_long = new MemberCollection();
            MemberCollection inner_cross = new MemberCollection();


            z_min = Minor_Analysis.TotalLoad_Analysis.Analysis.Joints.MinZ;
            double z_max = Minor_Analysis.TotalLoad_Analysis.Analysis.Joints.MaxZ;


            //Store inner and outer Long Girder
            for (i = 0; i < sort_membs.Count; i++)
            {
                if (((sort_membs[i].StartNode.Z == z_min) || (sort_membs[i].StartNode.Z == z_max)) &&
                    sort_membs[i].StartNode.Z == sort_membs[i].EndNode.Z)
                {
                    outer_long.Add(sort_membs[i]);
                }
                else if (((sort_membs[i].StartNode.Z != z_min) && (sort_membs[i].StartNode.Z != z_max)) &&
                    sort_membs[i].StartNode.Z == sort_membs[i].EndNode.Z)
                {
                    inner_long.Add(sort_membs[i]);
                }
            }

            List<int> Outer_Joints = new List<int>();
            List<int> Inner_Joints = new List<int>();

            for (i = 0; i < outer_long.Count; i++)
            {
                if (Outer_Joints.Contains(outer_long[i].EndNode.NodeNo) == false)
                    Outer_Joints.Add(outer_long[i].EndNode.NodeNo);
                if (Outer_Joints.Contains(outer_long[i].StartNode.NodeNo) == false)
                    Outer_Joints.Add(outer_long[i].StartNode.NodeNo);
            }

            for (i = 0; i < inner_long.Count; i++)
            {
                if (Inner_Joints.Contains(inner_long[i].EndNode.NodeNo) == false)
                    Inner_Joints.Add(inner_long[i].EndNode.NodeNo);
                if (Inner_Joints.Contains(inner_long[i].StartNode.NodeNo) == false)
                    Inner_Joints.Add(inner_long[i].StartNode.NodeNo);
            }
            Outer_Joints.Sort();
            Inner_Joints.Sort();


            string inner_long_text = "";
            string outer_long_text = "";
            int last_val = 0;
            int to_val = 0;
            int from_val = 0;

            last_val = Outer_Joints[0];
            from_val = last_val;
            bool flag_1 = false;
            for (i = 0; i < Outer_Joints.Count; i++)
            {
                if (i < Outer_Joints.Count - 1)
                {
                    if ((Outer_Joints[i] + 1) == (Outer_Joints[i + 1]))
                    {
                        if (flag_1 == false)
                        {
                            from_val = Outer_Joints[i];
                        }
                        flag_1 = true;
                        to_val = Outer_Joints[i + 1];
                    }
                    else
                    {
                        if (flag_1)
                        {
                            outer_long_text = from_val + " TO " + to_val + " ";
                            flag_1 = false;
                        }
                        else
                        {
                            outer_long_text = outer_long_text + " " + last_val;
                        }
                    }
                    last_val = Outer_Joints[i];
                }
                else
                {
                    if (flag_1)
                    {
                        outer_long_text += from_val + " TO " + to_val + " ";
                        flag_1 = false;
                    }
                    else
                    {
                        outer_long_text = outer_long_text + " " + last_val;
                    }
                }
            }

            for (i = 0; i < Inner_Joints.Count; i++)
            {
                if (i < Inner_Joints.Count - 1)
                {
                    if ((Inner_Joints[i] + 1) == (Inner_Joints[i + 1]))
                    {
                        if (flag_1 == false)
                        {
                            from_val = Inner_Joints[i];
                        }
                        flag_1 = true;
                        to_val = Inner_Joints[i + 1];
                    }
                    else
                    {
                        if (flag_1)
                        {
                            inner_long_text = from_val + " TO " + to_val + " ";
                            flag_1 = false;
                        }
                        else
                        {
                            inner_long_text = inner_long_text + " " + last_val;
                        }
                    }
                    last_val = Inner_Joints[i];
                }
                else
                {
                    if (flag_1)
                    {
                        inner_long_text += from_val + " TO " + to_val + " ";
                        flag_1 = false;
                    }
                    else
                    {
                        inner_long_text = inner_long_text + " " + last_val;
                    }
                }
            }
            list_arr.Add(inner_long_text + " FY  -" + load.ToString("0.000"));
            list_arr.Add(outer_long_text + " FY  -" + (load / 2.0).ToString("0.000"));

            return list_arr;
        }
        public void Default_Moving_LoadData(DataGridView dgv_live_load)
        {
            List<string> list = new List<string>();
            List<string> lst_spc = new List<string>();
            dgv_live_load.Rows.Clear();
            int i = 0;
            list.Clear();
            list.Add(string.Format("TYPE 1, IRCCLASSA"));
            list.Add(string.Format("AXLE LOAD IN TONS , 2.7,2.7,11.4,11.4,6.8,6.8,6.8,6.8"));
            list.Add(string.Format("AXLE SPACING IN METRES, 1.10,3.20,1.20,4.30,3.00,3.00,3.00"));
            list.Add(string.Format("AXLE WIDTH IN METRES, 1.800"));
            list.Add(string.Format("IMPACT FACTOR, 1.179"));
            list.Add(string.Format(""));
            list.Add(string.Format("TYPE 2, IRC70RTRACK"));
            list.Add(string.Format("AXLE LOAD IN TONS, 7.0,7.0,7.0,7.0,7.0,7.0,7.0,7.0,7.0,7.0"));
            list.Add(string.Format("AXLE SPACING IN METRES, 0.457,0.457,0.457,0.457,0.457,0.457,0.457,0.457,0.457"));
            list.Add(string.Format("AXLE WIDTH IN METRES, 2.900"));
            list.Add(string.Format("IMPACT FACTOR, 1.25"));
            list.Add(string.Format(""));
            list.Add(string.Format("TYPE 3, IRC70RWHEEL"));
            list.Add(string.Format("AXLE LOAD IN TONS,17.0,17.0,17.0,17.0,12.0,12.0,8.0"));
            list.Add(string.Format("AXLE SPACING IN METRES,1.37,3.05,1.37,2.13,1.52,3.96"));
            list.Add(string.Format("AXLE WIDTH IN METRES,2.900"));
            list.Add(string.Format("IMPACT FACTOR, 1.25"));
            list.Add(string.Format(""));
            list.Add(string.Format("TYPE 4, IRC70RW40TBL"));
            list.Add(string.Format("AXLE LOAD IN TONS,10.0,10.0"));
            list.Add(string.Format("AXLE SPACING IN METRES,1.93"));
            list.Add(string.Format("AXLE WIDTH IN METRES,2.790"));
            list.Add(string.Format("IMPACT FACTOR, 1.10"));
            list.Add(string.Format(""));
            list.Add(string.Format("TYPE 5, IRC70RW40TBM"));
            list.Add(string.Format("AXLE LOAD IN TONS,5.0,5.0,5.0,5.0"));
            list.Add(string.Format("AXLE SPACING IN METRES,0.795,0.38,0.795"));
            list.Add(string.Format("AXLE WIDTH IN METRES,2.790"));
            list.Add(string.Format("IMPACT FACTOR, 1.10"));
            list.Add(string.Format(""));


            for (i = 0; i < dgv_live_load.ColumnCount; i++)
            {
                lst_spc.Add("");
            }
            for (i = 0; i < list.Count; i++)
            {
                dgv_live_load.Rows.Add(lst_spc.ToArray());
            }

            MyList mlist = null;
            for (i = 0; i < list.Count; i++)
            {
                mlist = new MyList(list[i], ',');

                if (list[i] == "")
                {
                    dgv_live_load.Rows[i].DefaultCellStyle.BackColor = Color.Cyan;
                }
                for (int j = 0; j < mlist.Count; j++)
                {
                    dgv_live_load[j, i].Value = mlist[j];
                }
            }
        }

        public void Default_Moving_Type_LoadData(DataGridView dgv_live_load)
        {
            List<string> lst_spcs = new List<string>();
            dgv_live_load.Rows.Clear();
            int i = 0;
            for (i = 0; i < dgv_live_load.ColumnCount; i++)
            {
                lst_spcs.Add("");
            }
            List<string> list = new List<string>();

            if (dgv_live_load.Name == dgv_long_loads.Name)
            {
                #region Long Girder
                list.Clear();
                list.Add(string.Format("LOAD 1,TYPE 3"));
                list.Add(string.Format("X,-13.4"));
                list.Add(string.Format("Z,1.5"));
                list.Add(string.Format(""));
                list.Add(string.Format("LOAD 2,TYPE 1"));
                list.Add(string.Format("X,-18.8"));
                list.Add(string.Format("Z,1.5"));
                list.Add(string.Format(""));
                list.Add(string.Format("LOAD 3,TYPE 1,TYPE 1"));
                list.Add(string.Format("X,-18.8,-18.8"));
                list.Add(string.Format("Z,1.5,4.5"));
                list.Add(string.Format(""));
                list.Add(string.Format("LOAD 4,TYPE 1,TYPE 1,TYPE 1"));
                list.Add(string.Format("X,-18.8,-18.8,-18.8"));
                list.Add(string.Format("Z,1.5,4.5,7.5"));
                list.Add(string.Format(""));
                list.Add(string.Format("LOAD 5,TYPE 1,TYPE 3"));
                list.Add(string.Format("X,-18.8,-13.4"));
                list.Add(string.Format("Z,1.5,4.5"));
                list.Add(string.Format(""));
                list.Add(string.Format("LOAD 6,TYPE 3,TYPE 1"));
                list.Add(string.Format("X,-13.4,-18.8"));
                list.Add(string.Format("Z,1.5,4.5"));
                list.Add(string.Format(""));
                //list.Add(string.Format("TOTAL LOAD,TYPE 1,TYPE 1,TYPE 1"));
                //list.Add(string.Format("X,-18.8,-18.8,-18.8"));
                //list.Add(string.Format("Z,1.5,4.5,7.5"));
                //list.Add(string.Format(""));
                #endregion
            }

            for (i = 0; i < list.Count; i++)
            {
 
                dgv_live_load.Rows.Add(lst_spcs.ToArray());
            }

            MyList mlist = null;
            for (i = 0; i < list.Count; i++)
            {
                mlist = new MyList(list[i], ',');

                if (list[i] == "")
                {
                    dgv_live_load.Rows[i].DefaultCellStyle.BackColor = Color.Cyan;
                }
                for (int j = 0; j < mlist.Count; j++)
                {
                    dgv_live_load[j, i].Value = mlist[j];
                }
            }

             
        }

        public void Deckslab_User_Input()
        {
         
        }



        public void Long_Girder_User_Input()
        {
            List<string> lst_input = new List<string>();
            #region user input
            lst_input.Add(string.Format("Distance between C/C Exp. Joint"));

            lst_input.Add(string.Format("Overhang of girder off the bearing"));
            lst_input.Add(string.Format("Overhang of slab off the bearing"));
            lst_input.Add(string.Format("Expansion Joint"));
            lst_input.Add(string.Format("Deck Width"));
            lst_input.Add(string.Format("Angle of skew"));

            lst_input.Add(string.Format("Width of outer railing"));
            lst_input.Add(string.Format("Width of Footpath"));
            lst_input.Add(string.Format("Width of Crash Barrier"));
            lst_input.Add(string.Format("Spacing of main girder c/c"));

            lst_input.Add(string.Format("Thk of deck slab"));
            lst_input.Add(string.Format("Thk of deck slab at overhang"));
            lst_input.Add(string.Format("Thk of wearing coat"));
            lst_input.Add(string.Format("Thk of wearing coat for Design "));

            lst_input.Add(string.Format("Cantilever slab thk at fixed end"));
            lst_input.Add(string.Format("Cantilever slab thk at free end"));
            lst_input.Add(string.Format("No of main girder"));
            lst_input.Add(string.Format("Depth of main girder"));
            lst_input.Add(string.Format("Flange width of girder"));
            lst_input.Add(string.Format("Web thk of girder at mid span"));
            lst_input.Add(string.Format("Web thk of girder at Support"));
            lst_input.Add(string.Format("Thickness of top flange"));
            lst_input.Add(string.Format("Thickness of top haunch at midspan"));
            lst_input.Add(string.Format("Thickness of top haunch at support"));
            lst_input.Add(string.Format("Bottom width of flange"));
            lst_input.Add(string.Format("Thickness of bottom flange"));
            lst_input.Add(string.Format("Thickness of bottom haunch"));
            lst_input.Add(string.Format("Length of varying portion"));
            lst_input.Add(string.Format("Length of solid portion"));
            lst_input.Add(string.Format("No of Intermediate cross girder"));


            lst_input.Add(string.Format("Web thk of Intermediate cross girder "));
            lst_input.Add(string.Format("Web thk of end cross girder "));
            lst_input.Add(string.Format("Grade of concrete                      "));
            lst_input.Add(string.Format("Grade of reinforcement"));


            lst_input.Add(string.Format("Partial factor of safety  (Basic and seismic)"));
            lst_input.Add(string.Format("Partial factor of safety Accidental "));
            lst_input.Add(string.Format("Coefficient to considerb the influence of the strength"));


            lst_input.Add(string.Format("Clear cover"));
            lst_input.Add(string.Format("Unit weight of dry concrete"));
            lst_input.Add(string.Format("Unit weight of wet concrete"));

            lst_input.Add(string.Format("Weight of Crash Barrier"));
            lst_input.Add(string.Format("Weight of Railing"));
            lst_input.Add(string.Format("Intensity of Load for shuttering"));


            lst_input.Add(string.Format("Partial factor of safety for basic and seismic"));
            lst_input.Add(string.Format("Partial factor of safety for Accidental"));

            lst_input.Add(string.Format("Es"));

            lst_input.Add(string.Format("Diameter of bar to be used for shear stirrups"));
            if (iApp.DesignStandard == eDesignStandard.BritishStandard)
                lst_input.Add(string.Format("Constant as per Section 6.2.3 Note 3 BS Eurocode2"));
            else
                lst_input.Add(string.Format("Constant as per cl. 10.3.1 of IRC 112"));

            lst_input.Add(string.Format("Strength reduction factor for concrete cracked in shear"));
            lst_input.Add(string.Format("Ratio of the longitudinal force in the new concrete and the total logitudinal force"));
            lst_input.Add(string.Format("Factor depends on the roughness of the interface"));
            lst_input.Add(string.Format("Angle of the reinforcement to the interface"));


            #endregion
            List<string> lst_inp_vals = new List<string>();
            #region Value
            lst_inp_vals.Add(string.Format("19.58"));

            lst_inp_vals.Add(string.Format("0.500"));
            lst_inp_vals.Add(string.Format("0.759"));
            lst_inp_vals.Add(string.Format("40.0"));
            lst_inp_vals.Add(string.Format("12.0"));
            lst_inp_vals.Add(string.Format("26.00"));

            lst_inp_vals.Add(string.Format("0.00"));
            lst_inp_vals.Add(string.Format("0.00"));
            lst_inp_vals.Add(string.Format("0.45"));
            lst_inp_vals.Add(string.Format("3.00"));

            lst_inp_vals.Add(string.Format("0.21"));
            lst_inp_vals.Add(string.Format("0.40"));
            lst_inp_vals.Add(string.Format("0.065"));
            lst_inp_vals.Add(string.Format("0.075"));

            lst_inp_vals.Add(string.Format("0.21"));
            lst_inp_vals.Add(string.Format("0.21"));
            lst_inp_vals.Add(string.Format("4"));
            lst_inp_vals.Add(string.Format("1.600"));
            lst_inp_vals.Add(string.Format("0.800"));
            lst_inp_vals.Add(string.Format("0.300"));
            lst_inp_vals.Add(string.Format("0.625"));
            lst_inp_vals.Add(string.Format("0.150"));
            lst_inp_vals.Add(string.Format("0.100"));
            lst_inp_vals.Add(string.Format("0.033"));
            lst_inp_vals.Add(string.Format("0.625"));
            lst_inp_vals.Add(string.Format("0.250"));
            lst_inp_vals.Add(string.Format("0.150"));
            lst_inp_vals.Add(string.Format("0.98"));
            lst_inp_vals.Add(string.Format("1.45"));
            lst_inp_vals.Add(string.Format("1.00"));


            lst_inp_vals.Add(string.Format("0.30"));
            lst_inp_vals.Add(string.Format("0.30"));
            lst_inp_vals.Add(string.Format(" M35"));
            lst_inp_vals.Add(string.Format("Fe 500"));


            lst_inp_vals.Add(string.Format("1.50"));
            lst_inp_vals.Add(string.Format("1.20"));
            lst_inp_vals.Add(string.Format("0.67"));


            lst_inp_vals.Add(string.Format("40.0"));
            lst_inp_vals.Add(string.Format("2.50"));
            lst_inp_vals.Add(string.Format("2.60"));

            lst_inp_vals.Add(string.Format("1.00"));
            lst_inp_vals.Add(string.Format("0.50"));
            lst_inp_vals.Add(string.Format("0.50"));


            lst_inp_vals.Add(string.Format("1.15"));
            lst_inp_vals.Add(string.Format("1"));

            lst_inp_vals.Add(string.Format("200000"));

            lst_inp_vals.Add(string.Format("16.00"));
            lst_inp_vals.Add(string.Format("1.00"));
            lst_inp_vals.Add(string.Format("0.60"));
            lst_inp_vals.Add(string.Format("1.00"));
            lst_inp_vals.Add(string.Format("0.70"));
            lst_inp_vals.Add(string.Format("90.000"));

            #endregion

            #region Input Units
            List<string> lst_units = new List<string>();
            lst_units.Add(string.Format("m"));

            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("mm"));
            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("deg."));

            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("m"));

            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("m"));

            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("Nos"));
            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("m"));
            lst_units.Add(string.Format("Nos"));


            lst_units.Add(string.Format("mm"));
            lst_units.Add(string.Format("mm"));
            lst_units.Add(string.Format("Mpa"));
            lst_units.Add(string.Format("Mpa"));


            lst_units.Add(string.Format(""));
            lst_units.Add(string.Format(""));
            lst_units.Add(string.Format(""));


            lst_units.Add(string.Format("mm"));
            lst_units.Add(string.Format("t/m3"));
            lst_units.Add(string.Format("t/m3"));

            lst_units.Add(string.Format("t/m"));
            lst_units.Add(string.Format("t/m"));
            lst_units.Add(string.Format("t/m2"));


            lst_units.Add(string.Format(""));
            lst_units.Add(string.Format(""));

            lst_units.Add(string.Format("Mpa"));

            lst_units.Add(string.Format(""));
            lst_units.Add(string.Format(""));
            lst_units.Add(string.Format(""));
            lst_units.Add(string.Format(""));
            lst_units.Add(string.Format(""));
            lst_units.Add(string.Format("deg"));

            #endregion Input Units

            dgv_long_user_input.Rows.Clear();
            for (int i = 0; i < lst_inp_vals.Count; i++)
            {
                dgv_long_user_input.Rows.Add(lst_input[i], lst_inp_vals[i], lst_units[i]);
            }
        }



        public void Cross_Girder_User_Input()
        {
            List<string> lst_input = new List<string>();
            #region user input
            lst_input.Add(string.Format("Depth of diaphragm "));
            lst_input.Add(string.Format("Breadth of Diaphragm "));
            lst_input.Add(string.Format("Characteristic strength of concrete (fck)       "));
            lst_input.Add(string.Format("Characteristic strength  of steel (fy)       "));
            //lst_input.Add(string.Format("sigma st"));
            lst_input.Add(string.Format("Permissible Stress in Steel"));
            lst_input.Add(string.Format("Maximum hogging moments "));
            lst_input.Add(string.Format("Maximum sagging moment "));
            lst_input.Add(string.Format("Maximum  Reaction due to  longitudinal girder "));

            #endregion
            List<string> lst_inp_vals = new List<string>();
            #region Value
            lst_inp_vals.Add(string.Format("1350"));
            lst_inp_vals.Add(string.Format("300"));
            lst_inp_vals.Add(string.Format("35"));
            lst_inp_vals.Add(string.Format("500"));
            lst_inp_vals.Add(string.Format("240"));
            lst_inp_vals.Add(string.Format("54.79"));
            lst_inp_vals.Add(string.Format("22.62"));
            lst_inp_vals.Add(string.Format("68.64"));
            #endregion

            #region Input Units
            List<string> lst_units = new List<string>();
            lst_units.Add(string.Format("mm"));
            lst_units.Add(string.Format("mm"));
            lst_units.Add(string.Format(""));
            lst_units.Add(string.Format(""));
            lst_units.Add(string.Format("Mpa"));
            lst_units.Add(string.Format("t-m"));
            lst_units.Add(string.Format("t-m"));
            lst_units.Add(string.Format("t"));
           
            #endregion Input Units

            dgv_cross_user_input.Rows.Clear();
            for (int i = 0; i < lst_inp_vals.Count; i++)
            {
                dgv_cross_user_input.Rows.Add(lst_input[i], lst_inp_vals[i], lst_units[i]);
            }
        }

        #endregion Bridge Deck Analysis Methods

        #region Long Girder Form Events

        private void cmb_concrete_grade_steel_grade_SelectedIndexChanged(object sender, EventArgs e)
        {
            ASTRAGrade astg = null;

            ComboBox cmb = sender as ComboBox;

            Control ctrl = sender as Control;

            if (ctrl.Name.ToLower().StartsWith("cmb_abut") ||
                ctrl.Name.ToLower().StartsWith("txt_abut"))
            {
                //astg = new ASTRAGrade(cmb_abut_fck.Text, cmb_abut_fy.Text);
                //txt_abut_sigma_c.Text = astg.sigma_c_N_sq_mm.ToString();
                //txt_abut_sigma_st.Text = astg.sigma_st_N_sq_mm.ToString();
            }

            else if (ctrl.Name.ToLower().StartsWith("cmb_rcc_pier") ||
                ctrl.Name.ToLower().StartsWith("txt_rcc_pier"))
            {
                astg = new ASTRAGrade(cmb_rcc_pier_fck.Text, cmb_rcc_pier_fy.Text);
                txt_rcc_pier_sigma_c.Text = astg.sigma_c_N_sq_mm.ToString();
                txt_rcc_pier_sigma_st.Text = astg.sigma_st_N_sq_mm.ToString();
            }
        }


        #endregion Long Girder

        #region Design of RCC Pier

        private void btn_RccPier_Report_Click(object sender, EventArgs e)
        {
            
            iApp.RunExe(rcc_pier.rep_file_name);
        }

        private void btn_Drawing_Click(object sender, EventArgs e)
        {
            Button b = sender as Button;

            if (b.Name == btn_dwg_rcc_abut.Name)
            {
                //Chiranjit [2012 11 08]
                //iApp.SetDrawingFile_Path(Abut.drawing_path, "Abutment_Cantilever", "TBeam_Abutment");
                iApp.RunViewer(Path.Combine(Drawing_Folder, "RCC Abutment Drawings"), "TBeam_Abutment");
            }
            else if (b.Name == btn_dwg_box_abutment.Name)
            {
                //Chiranjit [2012 11 08]
                iApp.RunViewer(Path.Combine(Drawing_Folder, "Box Type Abutment Drawings"), "BOX_ABUTMENT");
            }
            else if (b.Name == btn_dwg_pier.Name)
            {
                //Chiranjit [2012 11 08]
                //iApp.RunViewer(Path.GetDirectoryName(rcc_pier.rep_file_name), "RCC_Pier_Default_Drawings");
                iApp.RunViewer(Path.Combine(Drawing_Folder, "RCC Pier Drawings"), "TBeam_Pier");
            }


        }
        #endregion Design of RCC Pier

        #region Chiranjit [2012 06 20]
        void Calculate_Interactive_Values()
        {
            //return;
            //double eff_dp = MyList.StringToDouble(txt_Ana_eff_depth.Text, 0.0);
            double eff_dp = Deff;
            double lg_spa = SMG;
            double len = MyList.StringToDouble(txt_Ana_L.Text, 0.0);
            double wd = MyList.StringToDouble(txt_Ana_B.Text, 0.0);
            double cant_wd = MyList.StringToDouble(txt_Ana_Ds.Text, 0.0);
            double long_bw = MyList.StringToDouble(txt_sec_in_mid_lg_bw.Text, 0.0);



            //Long Girder
         
            //Cross Girders

            int lng_no = (int)((wd - 2 * cant_wd) / lg_spa);
            int cg_no = (int)((len) / lg_spa);
            lng_no++;
            cg_no++;
 
            //txt_Cross_grade_concrete.Text = cmb_Long_concrete_grade.Text;
            //txt_Cross_grade_steel.Text = cmb_Long_Steel_Grade.Text;


             
            
            //Abutment
            //txt_abut_DMG.Text = ((MyList.StringToDouble(txt_sec_in_mid_lg_D.Text, 0.0) / 1000) + 0.2).ToString("f3");
            //txt_abut_DMG.Text = (DMG + Ds - MyList.StringToDouble(txt_abut_d3.Text, 0.0)).ToString("f3");
            //txt_abut_B.Text = wd.ToString("f3");
            ////txt_abut_concrete_grade.Text = cmb_Long_concrete_grade.Text;
            ////txt_abut_steel_grade.Text = cmb_Long_Steel_Grade.Text;
            //txt_abut_gamma_c.Text = txt_Ana_gamma_c_dry.Text;
            //txt_abut_L.Text = len.ToString("f3");


            //RCC Pier Form 1
            txt_RCC_Pier_L.Text = len.ToString("f3");
            txt_RCC_Pier_CW.Text = txt_Ana_CW.Text;
            txt_RCC_Pier_DMG.Text = DMG.ToString("f3");
            txt_RCC_Pier_Ds.Text = Ds.ToString("f3");
            txt_RCC_Pier__B.Text = wd.ToString("f3");
            txt_RCC_Pier_NMG.Text = lng_no.ToString("f0");
            txt_RCC_Pier_NP.Text = lng_no.ToString("f0");
            txt_RCC_Pier___B.Text = (wd - 2.0).ToString("f3");


            double B9 = MyList.StringToDouble(txt_RCC_Pier_B9.Text, 0.0);
            double B10 = MyList.StringToDouble(txt_RCC_Pier_B10.Text, 0.0);
            double B11 = MyList.StringToDouble(txt_RCC_Pier_B11.Text, 0.0);
            double B12 = MyList.StringToDouble(txt_RCC_Pier_B12.Text, 0.0);
            double B14 = MyList.StringToDouble(txt_RCC_Pier___B.Text, 0.0);

            //txt_RCC_Pier_D.Text = ((B10 + B12) / 2.0).ToString("f3");
            //txt_RCC_Pier_b.Text = ((B9 + B11) / 2.0).ToString("f3");



            //txt_pier_2_SBC.Text = txt_abut_p_bearing_capacity.Text;
            //txt_pier_2_SC.Text = txt_abut_sc.Text;
            txt_pier_2_B16.Text = ((B14 - B12) / 2.0).ToString("f3");
        }
        
        void Text_Changed()
        {
            txt_Ana_NMG.Text = cmb_NMG.Text;

            SMG = (B - CL - CR) / (NMG - 1);
            double SCG = L / (NCG - 1);

            double Bb = MyList.StringToDouble(txt_sec_in_mid_lg_bwf.Text, 0.65);
            double Db = MyList.StringToDouble(txt_sec_in_mid_lg_D4.Text, 0.65);

            //Chiranjit [2012 12 26]
            //DMG = L / 10.0;
            //DCG = DMG - 0.4;

            //txt_LL_load_gen.Text = ((L + Math.Abs(MyList.StringToDouble(txt_Ana_X.Text, 0.0))) / (MyList.StringToDouble(txt_XINCR.Text, 0.0))).ToString("f0");
            txt_LL_load_gen.Text = ((L) / (MyList.StringToDouble(txt_XINCR.Text, 0.0))).ToString("f0");


            leff = L - eg / 1000.0 - 2 * os;

            //Chiranjit comment this line on 2014/02/12 (yyyy/mm/dd)

            //if (Bs != 0.0)
            //    CW = B - Bs;
            //else
            //    CW = B - 2 * Wp;

            if (rbtn_crash_barrier.Checked)
                CW = B - 2 * Wc;
            else if (rbtn_footpath.Checked)
            {
                if (chk_fp_left.Checked && !chk_fp_right.Checked)
                    CW = B - Wf - Wk;
                else if (!chk_fp_left.Checked && chk_fp_right.Checked)
                    CW = B - Wf - Wk;
                else
                    CW = B - 2 * Wf;
            }
            
            Calculate_Interactive_Values();
            Ana_Initialize_Analysis_InputData();
            Calculate_Load_Computation();


            txt_sec_in_mid_lg_w.Text = txt_Ana_SMG.Text;
            txt_sec_in_mid_lg_Ds.Text = txt_Ana_Ds.Text;
            txt_sec_in_mid_lg_D.Text = txt_Ana_DMG.Text;

            txt_sec_out_mid_lg_W.Text = txt_Ana_SMG.Text;
            txt_sec_out_mid_lg_Ds.Text = txt_Ana_Ds.Text;
            txt_sec_out_mid_lg_D.Text = txt_Ana_DMG.Text;

            txt_sec_in_sup_lg_w.Text = txt_Ana_SMG.Text;
            txt_sec_in_sup_lg_Ds.Text = txt_Ana_Ds.Text;
            txt_sec_in_sup_lg_D.Text = txt_Ana_DMG.Text;

            txt_sec_out_sup_lg_W.Text = txt_Ana_SMG.Text;
            txt_sec_out_sup_lg_Ds.Text = txt_Ana_Ds.Text;
            txt_sec_out_sup_lg_D.Text = txt_Ana_DMG.Text;


            txt_sec_end_cg_d.Text = txt_Ana_DCG.Text;
            txt_sec_end_cg_Ds.Text = txt_Ana_Ds.Text;
            txt_sec_end_cg_D1.Text = (Dso - Ds).ToString("f2");

            txt_sec_int_cg_d.Text = txt_Ana_DCG.Text;
            txt_sec_int_cg_Ds.Text = txt_Ana_Ds.Text;




            Calculate_Section_Properties();

            //Chiranjit [2014 09 06]
            txt_deck_width.Text = B.ToString();

            #region Main Girder Inputs
            if (dgv_long_user_input.RowCount >= 38)
            {
                dgv_long_user_input[1, 0].Value = txt_Ana_L.Text;
                dgv_long_user_input[1, 1].Value = txt_Ana_og.Text;
                dgv_long_user_input[1, 2].Value = txt_Ana_os.Text;
                dgv_long_user_input[1, 3].Value = txt_Ana_eg.Text;
                dgv_long_user_input[1, 4].Value = txt_Ana_B.Text;
                dgv_long_user_input[1, 5].Value = txt_Ana_ang.Text;
                //dgv_long_user_input[1, 6].Value = txt_Ana_CW.Text;
                dgv_long_user_input[1, 7].Value = txt_Ana_Wf.Text;
                dgv_long_user_input[1, 8].Value = txt_Ana_Wc.Text;
                dgv_long_user_input[1, 9].Value = SMG.ToString("f3");
                dgv_long_user_input[1, 10].Value = txt_Ana_Ds.Text;
                dgv_long_user_input[1, 11].Value = txt_Ana_Dso.Text;
                dgv_long_user_input[1, 13].Value = txt_Ana_Dw.Text;
                dgv_long_user_input[1, 16].Value = txt_Ana_NMG.Text;
                dgv_long_user_input[1, 17].Value = txt_sec_in_mid_lg_D.Text;

                dgv_long_user_input[1, 18].Value = txt_sec_in_mid_lg_wtf.Text;
                dgv_long_user_input[1, 19].Value = txt_sec_in_mid_lg_bw.Text;
                dgv_long_user_input[1, 20].Value = txt_sec_in_sup_lg_bw.Text;
                dgv_long_user_input[1, 21].Value = txt_sec_in_mid_lg_D1.Text;
                dgv_long_user_input[1, 22].Value = txt_sec_in_mid_lg_D2.Text;
                dgv_long_user_input[1, 23].Value = txt_sec_in_sup_lg_D2.Text;

                dgv_long_user_input[1, 24].Value = txt_sec_in_mid_lg_bwf.Text;
                dgv_long_user_input[1, 25].Value = txt_sec_in_mid_lg_D4.Text;
                dgv_long_user_input[1, 26].Value = txt_sec_in_mid_lg_D3.Text;
                dgv_long_user_input[1, 27].Value = txt_Ana_Lvp.Text;
                dgv_long_user_input[1, 28].Value = txt_Ana_Lsp.Text;
                dgv_long_user_input[1, 29].Value = (NCG - 2);
                dgv_long_user_input[1, 30].Value = txt_sec_int_cg_bw.Text;
                dgv_long_user_input[1, 31].Value = txt_sec_end_cg_bw.Text;

                dgv_long_user_input[1, 38].Value = (Y_c_dry / 10.0).ToString("f3");
                dgv_long_user_input[1, 39].Value = (Y_c_wet / 10.0).ToString("f3");
                dgv_long_user_input[1, 40].Value = (wgc / 10.0).ToString("f3");
                dgv_long_user_input[1, 41].Value = (wgr / 10.0).ToString("f3");
                dgv_long_user_input[1, 42].Value = (ils / 10.0).ToString("f3");



                dgv_long_user_input[1, 0].Style.ForeColor = Color.Red;

                dgv_long_user_input[1, 0].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 1].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 2].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 3].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 4].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 5].Style.ForeColor = Color.Red;
                //dgv_long_user_input[1, 6].Value = txt_Ana_CW.Text;
                dgv_long_user_input[1, 7].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 8].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 9].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 10].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 11].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 13].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 16].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 17].Style.ForeColor = Color.Red;

                dgv_long_user_input[1, 18].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 19].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 20].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 21].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 22].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 23].Style.ForeColor = Color.Red;

                dgv_long_user_input[1, 24].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 25].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 26].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 27].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 28].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 29].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 30].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 31].Style.ForeColor = Color.Red;

                dgv_long_user_input[1, 38].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 39].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 40].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 41].Style.ForeColor = Color.Red;
                dgv_long_user_input[1, 42].Style.ForeColor = Color.Red;



            }
            #endregion Main Girder Inputs

            #region Cross Girder Inputs

            //dgv_cross_user_input[1, 0].Value = (SCG * 1000).ToString("f2");
            dgv_cross_user_input[1, 0].Value = (DCG * 1000).ToString("f2");
            dgv_cross_user_input[1, 0].Style.ForeColor = Color.Red;
            dgv_cross_user_input[1, 1].Value = (BCG * 1000).ToString("f2");
            dgv_cross_user_input[1, 1].Style.ForeColor = Color.Red;

            dgv_cross_user_input[1, 5].Style.ForeColor = Color.Red;
            dgv_cross_user_input[1, 6].Style.ForeColor = Color.Red;
            dgv_cross_user_input[1, 7].Style.ForeColor = Color.Red;


            #endregion Cross Girder Inputs

            #region Deck Slab Inputs
            if (uC_Deckslab_IS1.dgv_deck_user_input.RowCount >= 30)
            {
                uC_Deckslab_IS1.dgv_deck_user_input[1, 0].Value = txt_Ana_L.Text;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 1].Value = txt_Ana_og.Text;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 2].Value = txt_Ana_os.Text;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 3].Value = txt_Ana_eg.Text;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 4].Value = txt_Ana_B.Text;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 5].Value = txt_Ana_ang.Text;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 6].Value = txt_Ana_CW.Text;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 8].Value = txt_Ana_Wf.Text;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 9].Value = txt_Ana_Wc.Text;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 10].Value = SMG.ToString("f3");
                uC_Deckslab_IS1.dgv_deck_user_input[1, 11].Value = txt_Ana_Ds.Text;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 12].Value = txt_Ana_Dso.Text;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 15].Value = txt_Ana_Dw.Text;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 16].Value = txt_Ana_NMG.Text;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 19].Value = txt_sec_in_mid_lg_wtf.Text;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 20].Value = (NCG - 2);
                uC_Deckslab_IS1.dgv_deck_user_input[1, 27].Value = (Y_c_dry / 10.0).ToString("f3");
                uC_Deckslab_IS1.dgv_deck_user_input[1, 28].Value = (Y_c_wet / 10.0).ToString("f3");
                uC_Deckslab_IS1.dgv_deck_user_input[1, 29].Value = (Y_w / 10.0).ToString("f3");
                uC_Deckslab_IS1.dgv_deck_user_input[1, 30].Value = (wgc / 10.0).ToString("f3");
                uC_Deckslab_IS1.dgv_deck_user_input[1, 31].Value = (wgr / 10.0).ToString("f3");
                uC_Deckslab_IS1.dgv_deck_user_input[1, 32].Value = (ils / 10.0).ToString("f3");


                uC_Deckslab_IS1.dgv_deck_user_input[1, 0].Style.ForeColor = Color.Red;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 1].Style.ForeColor = Color.Red;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 2].Style.ForeColor = Color.Red;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 3].Style.ForeColor = Color.Red;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 4].Style.ForeColor = Color.Red;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 5].Style.ForeColor = Color.Red;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 6].Style.ForeColor = Color.Red;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 8].Style.ForeColor = Color.Red;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 9].Style.ForeColor = Color.Red;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 10].Style.ForeColor = Color.Red;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 11].Style.ForeColor = Color.Red;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 12].Style.ForeColor = Color.Red;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 15].Style.ForeColor = Color.Red;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 16].Style.ForeColor = Color.Red;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 19].Style.ForeColor = Color.Red;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 20].Style.ForeColor = Color.Red;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 27].Style.ForeColor = Color.Red;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 28].Style.ForeColor = Color.Red;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 29].Style.ForeColor = Color.Red;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 30].Style.ForeColor = Color.Red;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 31].Style.ForeColor = Color.Red;
                uC_Deckslab_IS1.dgv_deck_user_input[1, 32].Style.ForeColor = Color.Red;




            }
            #endregion Deck Slab Inputs


            uC_Deckslab_BS1.b = B;

            uC_Deckslab_BS1.girder_no = NMG;

            uC_Deckslab_BS1.h = Ds * 1000;



            uC_RCC_Abut1.Length = L;
            uC_RCC_Abut1.Width = B;
            uC_RCC_Abut1.Overhang = og;

        }
        private void txt_Ana_width_TextChanged(object sender, EventArgs e)
        {
            //Calculate_Interactive_Values();
            Text_Changed();
            //Chiranjit comment this line on 2014/02/12 (yyyy/mm/dd)
            //if (((TextBox)sender).Name == txt_Ana_B.Name)
                //CW = B - 2.0;
        }


        string _Dw = "";
        string _Yw = "";
        //Chiranjit [2012 06 20]
        private void rbtn_CheckedChanged(object sender, EventArgs e)
        {
            Control rbtn = sender as Control;


            if (rbtn.Name == chk_fp_left.Name)
            {
                if (!chk_fp_left.Checked && !chk_fp_right.Checked)
                    chk_fp_right.Checked = true;
            }
            else if (rbtn.Name == chk_fp_right.Name)
            {
                if (!chk_fp_left.Checked && !chk_fp_right.Checked)
                    chk_fp_left.Checked = true;
            }

            if (rbtn.Name == chk_WC.Name)
            {
                grb_ana_wc.Enabled = chk_WC.Checked;
                if (grb_ana_wc.Enabled == false)
                {

                    _Dw = txt_Ana_Dw.Text;
                    _Yw = txt_Ana_gamma_w.Text;

                    txt_Ana_Dw.Text = "0.000";
                    txt_Ana_gamma_w.Text = "0.000";
                }
                else
                {
                    //txt_Ana_Dw.Text = "0.080";
                    //txt_Ana_gamma_w.Text = "22";

                    txt_Ana_Dw.Text = _Dw;
                    txt_Ana_gamma_w.Text = _Yw;
                }
            }
            else if (rbtn.Name == rbtn_crash_barrier.Name)
            {
                grb_ana_parapet.Enabled = rbtn_crash_barrier.Checked;
                if (!rbtn_crash_barrier.Checked)
                {
                    txt_Ana_Hc.Text = "0.000";
                    txt_Ana_Wc.Text = "0.000";
                }
                else
                {
                    txt_Ana_Hc.Text = "1.200";
                    txt_Ana_Wc.Text = "0.500";
                }
            }
            else if (rbtn.Name == rbtn_footpath.Name)
            {
                grb_ana_sw_fp.Enabled = rbtn_footpath.Checked;
                if (!rbtn_footpath.Checked)
                {
                    txt_Ana_Wf.Text = "0.000";
                    txt_Ana_Hf.Text = "0.000";
                    txt_Ana_Wk.Text = "0.000";
                    txt_Ana_Wr.Text = "0.000";
                }
                else
                {
                    txt_Ana_Wf.Text = "1.000";
                    txt_Ana_Hf.Text = "0.250";
                    txt_Ana_Wk.Text = "0.500";
                    txt_Ana_Wr.Text = "0.100";
                }
            }

            if (rbtn_crash_barrier.Checked)
            {
                pic_diagram.BackgroundImage = LimitStateMethod.Properties.Resources.case_1;

            }
            else if (rbtn_footpath.Checked)
            {
                if (chk_fp_left.Checked && chk_fp_right.Checked)
                    pic_diagram.BackgroundImage = LimitStateMethod.Properties.Resources.case_2;
                else if (chk_fp_left.Checked && !chk_fp_right.Checked)
                    pic_diagram.BackgroundImage = LimitStateMethod.Properties.Resources.case_3;
                else if (!chk_fp_left.Checked && chk_fp_right.Checked)
                    pic_diagram.BackgroundImage = LimitStateMethod.Properties.Resources.case_4;
            }

            Text_Changed();
        }
        public void Calculate_Load_Computation()
        {
            List<string> list = new List<string>();
            List<string> member_load = new List<string>();


            double SMG, SCG, wi1, wi2, wi3, wi4, NIG, NIM, wiu, wo1, wo2, wo3, wo4, wo5, wo6, wo7, NOG, NOM;
            double wou, wc1, NIGJ, NIMJ, wjl, C;

            list.Add(string.Format(""));
            list.Add(string.Format("--------------------------------------------------------------------"));
            list.Add(string.Format("ASTRA Load Computation for RCC T - Girder Bridge"));
            list.Add(string.Format("--------------------------------------------------------------------"));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Spacing of main long girders "));
            SMG = (B - CL - CR) / (NMG - 1);
            list.Add(string.Format("SMG = (B-CL-CR)/(NMG-1) = ({0:f3}-{1:f3}-{2:f3})/({3:f0}-1) = {4:f3} m.",
                B, CL, CR, NMG, SMG));
            list.Add(string.Format(""));
            list.Add(string.Format("//Spacing of cross girders "));
            SCG = L / (NCG - 1);
            list.Add(string.Format("SCG = L/(NCG-1) = {0:f3}/({1:f0}-1) = {2:f3} m.",
                L, NCG, SCG));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//UDL in all main long Inner Girder members"));
            list.Add(string.Format(""));
            list.Add(string.Format("//Load from RCC Deck Slab"));
            wi1 = SMG * SCG * (Ds * Y_c_dry + Dw * Y_w);
            list.Add(string.Format("wi1 = SMG*SCG*(Ds*Y_c + Dw*Y_w) "));
            list.Add(string.Format("   = {0:f3}*{1:f3}*({2:f3}*{3:f3}+{4:f3}*{5:f3}) ",
                SMG, SCG, Ds, Y_c_dry, Dw, Y_w));
            list.Add(string.Format("   = {0:f3} kN.", wi1));
            list.Add(string.Format(""));
            list.Add(string.Format("//Load of self weight of main long girder"));
            wi2 = SCG * BMG * DMG * Y_c_dry;
            list.Add(string.Format("wi2 = SCG*BMG*DMG*Y_c = {0:f3}*{1:f3}*{2:f3}*{3:f3} = {4:f3} kN.",
                SCG, BMG, DMG, Y_c_dry, wi2));
            list.Add(string.Format(""));
            list.Add(string.Format("//Total load on main long girders"));
            wi3 = wi1 + wi2;
            list.Add(string.Format("wi3 = wi1 + wi2 = {0:f3} + {1:f3} = {2:f3} kN.",
                wi1, wi2, wi3));
            list.Add(string.Format(""));
            list.Add(string.Format("//UDL"));
            wi4 = wi3 / SCG;
            list.Add(string.Format("wi4 = wi3/SCG = {0:f3} / {1:f3} = {2:f3} kN/m.",
                wi3, SCG, wi4));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Total inner members segments of main long girders"));
            NIG = NMG * (NCG - 1);
            list.Add(string.Format("NIG = NMG*(NCG - 1) = {0:f0} * ({1:f0}-1) = {2:f0} nos.",
                NMG, NCG, NIG));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            NIM = 70;
            list.Add(string.Format("//Total inner members segments of main long girders in model (constant value)"));
            list.Add(string.Format("NIM = 70 nos."));
            list.Add(string.Format(""));
            list.Add(string.Format("//UDL in inner members segments of main long girders in model "));
            wiu = wi4 * NIG / NIM;
            list.Add(string.Format("wiu = wi4*NIG/NIM = {0:f3} * {1}/70 = {2:f3} kN/m. = {3:f3} Ton/m.",
                wi4, NIG, wiu, (wiu = wiu / 10.0)));
            list.Add(string.Format(""));



            list.Add(string.Format(""));
            list.Add(string.Format("//Factored UDL"));

            list.Add(string.Format("wiu = wiu*swf = {0:f3} * {1:f3} = {2:f3} Ton/m.",
                wiu, swf, (wiu = wiu * swf)));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//In Analysis Input Data file UDL in all inner Girder members is to be mentioned as"));
            list.Add(string.Format(""));
            list.Add(string.Format("                **********************************"));
            //member_load.Add(string.Format("MEMBER LOAD "));
            member_load.Add(string.Format("131 TO 200 UNI GY -{0:f4}", wiu));

            //list.Add(string.Format("                MEMBER LOAD "));
            list.Add(string.Format("                131 TO 200 UNI GY -{0:f4}", wiu));
            list.Add(string.Format("                **********************************"));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//UDL in all main long Outer Girder members"));
            list.Add(string.Format(""));
            //list.Add(string.Format("if(CL > CR) then (C=CL) else (C=CR)"));
            if (CL > CR) C = CL; else C = CR;
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Load from RCC Deck Slab and Wearing Course"));
            wo1 = ((SMG / 2) + C) * SCG * (Ds * Y_c_dry + Dw * Y_w);
            list.Add(string.Format("wo1 = [(SMG/2) + C]*SCG*(Ds*Y_c + Dw*Y_w) "));
            list.Add(string.Format("   = ({0:f3}/2 + {1:f3})*{2:f3}*({3:f3}*{4:f3}+{5:f3}*{6:f3}) ",
                SMG, C, SCG, Ds, Y_c_dry, Dw, Y_w));
            list.Add(string.Format("   = {0:f3} kN.", wo1));
            list.Add(string.Format(""));
            list.Add(string.Format("//Load of self weight of main long girder"));
            wo2 = SCG * BMG * DMG * Y_c_dry;
            list.Add(string.Format("wo2 = SCG*BMG*DMG*Y_c = {0:f3}*{1:f3}*{2:f3}*{3:f3} = {4:f3} kN.",
                SCG, BMG, DMG, Y_c_dry, wo2));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Load of one side Parapet wall"));
            wo3 = SCG * Hc * Wc * Y_c_dry;
            list.Add(string.Format("wo3 = SCG*Hp*Wp*Y_c = {0:f3}*{1:f3}*{2:f3}*{3:f3} = {4:f3} kN.",
               SCG, Hc, Wc, Y_c_dry, wo3));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Load of Side walk")); wo4 = SCG * Wf * Hs * Y_c_dry;
            list.Add(string.Format("wo4 = SCG*Bs*Hs*Y_c = {0:f3}*{1:f3}*{2:f3}*{3:f3} = {4:f3} kN.",
                SCG, Wf, Hs, Y_c_dry, wo4));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Load of Side Walk Parapet wall")); wo5 = SCG * Wr * Wk * Y_c_dry;
            list.Add(string.Format("wo5 = SCG*Hps*Wps*Y_c = {0:f3}*{1:f3}*{2:f3}*{3:f3} = {4:f3} kN.",
                SCG, Wr, Wk, Y_c_dry, wo5));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Total load on main long girders ")); wo6 = wo1 + wo2 + wo3 + wo4 + wo5;
            list.Add(string.Format("wo6 = wo1 + wo2 + wo3 + wo4 + wo5 "));
            list.Add(string.Format("   = {0:f3} + {1:f3} + {2:f3} + {3:f3} + {4:f3} ",
                wo1, wo2, wo3, wo4, wo5));
            list.Add(string.Format("   = {0:f3} kN.", wo6));
            list.Add(string.Format(""));
            list.Add(string.Format("//UDL")); wo7 = wo6 / SCG;
            list.Add(string.Format("wo7 = wo6/SCG = {0:f3}/{1:f3} = {2:f3} kN/m.", wo6, SCG, wo7));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Total outer members segments of main long girders")); NOG = 2 * (NCG - 1);
            list.Add(string.Format("NOG = 2*(NCG - 1) = 2*({0:f0}-1) = {1:f0} nos.", NCG, NOG));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Total inner members segments of main long girders in model (constant value)"));
            NOM = 20;
            list.Add(string.Format("NOM = 20 nos."));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//UDL in inner members segments of main long girders in model "));
            wou = wo7 * NOG / NOM;
            list.Add(string.Format("wou = wo7*NOG/NOM = {0:f3}*{1:f0}/{2} = {3:f3} kN/m. = {4:f3} Ton/m.",
                wo7, NOG, NOM, wou, (wou = wou / 10.0)));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Factored UDL"));
            list.Add(string.Format("wou = wou*swf = {0:f3}*{1:f3} = {2:f4} Ton/m.",
                wou, swf, (wou = wou * swf)));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//In Analysis Input Data file UDL in all inner Girder members is to be mentioned as"));
            list.Add(string.Format(""));
            list.Add(string.Format("                **********************************"));
            list.Add(string.Format("                MEMBER LOAD "));
            member_load.Add(string.Format("121 TO 130 UNI GY -{0:f4}", wou));
            member_load.Add(string.Format("201 TO 210 UNI GY -{0:f4}", wou));
            list.Add(string.Format("                121 TO 130 UNI GY -{0:f4}", wou));
            list.Add(string.Format("                201 TO 210 UNI GY -{0:f4}", wou));
            list.Add(string.Format("                **********************************"));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Concentrated JOINT LOADS in all main long Inner and Outer Girder members"));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Self weight of Cross Girders"));
            list.Add(string.Format("")); wc1 = SMG * DCG * BCG * Y_c_dry;
            list.Add(string.Format("wc1 = SMG*DCG*BCG*Y_c = {0:f3}*{1:f3}*{2:f3}*{3:f3} = {4:f3} kN.",
                SMG, DCG, BCG, Y_c_dry, wc1));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Total number of Inner Joints"));
            list.Add(string.Format("")); NIGJ = NMG * NCG;
            list.Add(string.Format("NIGJ = NMG*NCG = {0:f0}*{1:f0}= {2:f0} nos.", NMG, NCG, NIGJ));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Total number of Inner Joints in model (Constant value, always)"));
            list.Add(string.Format("")); NIMJ = 81;
            list.Add(string.Format("NIMJ = 81"));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Joint Loads applicable to all inner joints in model"));
            list.Add(string.Format("")); wjl = wc1 * NIGJ / NIMJ;
            list.Add(string.Format("wjl = wc1*NIGJ/NIMJ = {0:f3}*{1}/{2} = {3:f3} kN. = {4:f3} Ton.",
                 wc1, NIGJ, NIMJ, wjl, (wjl = wjl / 10.0)));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Factored Joint Load"));
            list.Add(string.Format("wjl = wjl*swf = {0:f3}*{1:f3} = {2:f4} Ton.",
                wjl, swf, (wjl = wjl * swf)));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//In Analysis Input Data file UDL in all inner Girder members is to be mentioned as"));
            list.Add(string.Format(""));
            list.Add(string.Format("                ***********************************"));
            list.Add(string.Format("                JOINT LOAD"));
            list.Add(string.Format("                13 TO 21 FZ -{0:f4}", wjl));
            list.Add(string.Format("                24 TO 32 FZ -{0:f4}", wjl));
            list.Add(string.Format("                35 TO 43 FZ -{0:f4}", wjl));
            list.Add(string.Format("                46 TO 54 FZ -{0:f4}", wjl));
            list.Add(string.Format("                57 TO 65 FZ -{0:f4}", wjl));
            list.Add(string.Format("                68 TO 76 FZ -{0:f4}", wjl));
            list.Add(string.Format("                79 TO 87 FZ -{0:f4}", wjl));
            list.Add(string.Format("                90 TO 98 FZ -{0:f4}", wjl));
            list.Add(string.Format("                101 TO 109 FZ -{0:f4}", wjl));


            //member_load.Add(string.Format("LOAD 2"));
            member_load.Add(string.Format("JOINT LOAD"));
            member_load.Add(string.Format("13 TO 21 FZ -{0:f4}", wjl));
            member_load.Add(string.Format("24 TO 32 FZ -{0:f4}", wjl));
            member_load.Add(string.Format("35 TO 43 FZ -{0:f4}", wjl));
            member_load.Add(string.Format("46 TO 54 FZ -{0:f4}", wjl));
            member_load.Add(string.Format("57 TO 65 FZ -{0:f4}", wjl));
            member_load.Add(string.Format("68 TO 76 FZ -{0:f4}", wjl));
            member_load.Add(string.Format("79 TO 87 FZ -{0:f4}", wjl));
            member_load.Add(string.Format("90 TO 98 FZ -{0:f4}", wjl));
            member_load.Add(string.Format("101 TO 109 FZ -{0:f4}", wjl));

            list.Add(string.Format(""));
            list.Add(string.Format("                ***********************************"));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//END OF LOAD COMPUTATION"));
            list.Add(string.Format(""));

            txt_member_load.Lines = member_load.ToArray();
            rtb_calc_load.Lines = list.ToArray();

            if(Path.GetFileName( user_path) == Project_Name)
            File.WriteAllLines(Path.Combine(user_path, "Load_Computation.txt"), list.ToArray());
            //iApp.RunExe(Path.Combine(user_path, "Load_Computation.txt"));
        }


        //Chiranjit [2013 05 03]
        List<double> deck_member_load = new List<double>();

        public void Calculate_Load_Computation_Old(string outer_girders, string inner_girders, List<string> joints_nos)
        {

            List<string> list = new List<string>();
            List<string> long_member_load = new List<string>();

            //Bridge_Analysis

            double SMG, SCG, wi1, wi2, wi3, wi4, NIG, NIM, wiu, wo1, wo2, wo3, wo4, wo5, wo6, wo7, NOG, NOM;
            double wou, wc1, NIGJ, NIMJ, wjl, C;

            list.Add(string.Format(""));
            list.Add(string.Format("--------------------------------------------------------------------"));
            list.Add(string.Format("ASTRA Load Computation for RCC T - Girder Bridge"));
            list.Add(string.Format("--------------------------------------------------------------------"));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Spacing of main long girders "));
            SMG = (B - CL - CR) / (NMG - 1);
            list.Add(string.Format("SMG = (B-CL-CR)/(NMG-1) = ({0:f3}-{1:f3}-{2:f3})/({3:f0}-1) = {4:f3} m.",
                B, CL, CR, NMG, SMG));
            list.Add(string.Format(""));
            list.Add(string.Format("//Spacing of cross girders "));
            SCG = L / (NCG - 1);
            list.Add(string.Format("SCG = L/(NCG-1) = {0:f3}/({1:f0}-1) = {2:f3} m.",
                L, NCG, SCG));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//UDL in all main long Inner Girder members"));
            list.Add(string.Format(""));



            list.Add(string.Format(""));
            list.Add(string.Format("//Load of self weight of main long girder"));
            wi1 = SCG * BMG * DMG * Y_c_dry;
            list.Add(string.Format("wi1 = SCG*BMG*DMG*Y_c = {0:f3}*{1:f3}*{2:f3}*{3:f3} = {4:f3} kN. = {5:f3} Ton",
                SCG, BMG, DMG, Y_c_dry, wi1, (wi1 = wi1 / 10)));
            list.Add(string.Format(""));
            list.Add(string.Format(""));



            list.Add(string.Format("load on main girder = wi1 / {0:f3} = {1:f3} / {0:f3} = {2:f3} Ton/m", SCG, wi1, (wi1 = wi1 / SCG)));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("LOAD 1 SELF WEIGHT"));
            list.Add(string.Format("MEMBER LOAD"));
            list.Add(string.Format("{0} UNI GY -{1:f4}", inner_girders, wi1));

            //member_load.Add(string.Format("LOAD 1 SELF WEIGHT"));
            //member_load.Add(string.Format("MEMBER LOAD"));
            //member_load.Add(string.Format("{0} UNI GY -{1:f4}", inner_girders, wi1));



            list.Add(string.Format("//Load from RCC Deck Slab"));
            wi2 = SMG * SCG * (Ds * Y_c_dry + Dw * Y_w);
            list.Add(string.Format("wi2 = SMG*SCG*(Ds*Y_c + Dw*Y_w) "));
            list.Add(string.Format("   = {0:f3}*{1:f3}*({2:f3}*{3:f3}+{4:f3}*{5:f3}) ",
                SMG, SCG, Ds, Y_c_dry, Dw, Y_w));
            list.Add(string.Format("   = {0:f3} kN.", wi2));
            wi2 = wi2 / 10;
            list.Add(string.Format(""));
            list.Add(string.Format("   = {0:f3} T.", wi2));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("load on main girder = wi2 / {0:f3} = {1:f3} / {0:f3} = {2:f3} Ton/m", SCG, wi2, (wi2 = wi2 / SCG)));

            
            list.Add(string.Format(""));
            //member_load.Add(string.Format("LOAD 1 SELF WEIGHT"));
            //member_load.Add(string.Format("MEMBER LOAD"));
            //member_load.Add(string.Format("{0} UNI GY -{1:f4}", inner_girders, wi1));
            //member_load.Add(string.Format("LOAD 2 DECK SLAB LOAD"));
            //member_load.Add(string.Format("MEMBER LOAD"));
            //member_load.Add(string.Format("{0} UNI GY -{1:f4}", inner_girders, wi2));


          
            //list.Add(string.Format(""));
            //list.Add(string.Format("//Factored UDL"));
            //wiu = wi1;
            //list.Add(string.Format("wiu = wiu*swf = {0:f3} * {1:f3} = {2:f3} Ton/m.",
            //    wiu, swf, (wiu = wiu * swf)));
            //list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//In Analysis Input Data file UDL in all inner Girder members is to be mentioned as"));
            list.Add(string.Format(""));
            list.Add(string.Format("                **********************************"));
            //member_load.Add(string.Format("MEMBER LOAD "));
            //member_load.Add(string.Format("{0} UNI GY -{1:f4}", inner_girders, wiu));

           
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//UDL in all main long Outer Girder members"));
            list.Add(string.Format(""));
            //list.Add(string.Format("if(CL > CR) then (C=CL) else (C=CR)"));
            if (CL > CR) C = CL; else C = CR;
            list.Add(string.Format(""));
            list.Add(string.Format(""));


            list.Add(string.Format("//Load from RCC Deck Slab"));
            wo1 = ((SMG / 2) + C) * SCG * (Ds * Y_c_dry);

            list.Add(string.Format("wo1 = [(SMG/2) + C]*SCG*(Ds*Y_c) "));
            list.Add(string.Format("   = ({0:f3}/2 + {1:f3})*{2:f3}*({3:f3}*{4:f3}+{5:f3}*{6:f3}) ",
                SMG, C, SCG, Ds, Y_c_dry, Dw, Y_w));
            list.Add(string.Format("   = {0:f3} kN.", wo1));
            list.Add(string.Format(""));
            wo1 = wo1 / 10;
            list.Add(string.Format("   = {0:f3} T.", wo1));
            list.Add(string.Format(""));
            list.Add(string.Format("load on main girder = wo1 /  {0:f3} = {1:f3}/ {0:f3} = {2:f3} Ton/m", SCG, wo1, (wo1 = wo1 / SCG)));
            list.Add(string.Format(""));



            list.Add(string.Format("//Load from Wearing Course"));
            double wo11 = C * SCG * (Dw * Y_w);



            list.Add(string.Format("wo1 = C *SCG*(Dw*Y_w) "));
            list.Add(string.Format("   = ({0:f3}/2 + {1:f3})*{2:f3}*({3:f3}*{4:f3}+{5:f3}*{6:f3}) ",
                SMG, C, SCG, Ds, Y_c_dry, Dw, Y_w));
            list.Add(string.Format("   = {0:f3} kN.", wo11));
            list.Add(string.Format(""));
            wo11 = wo11 / 10;
            list.Add(string.Format("   = {0:f3} T.", wo11));
            list.Add(string.Format(""));
            list.Add(string.Format("load on main girder = wo1 / {0:f3} = {1:f3}/{0:f3} = {2:f3} Ton", SCG, wo11, (wo11 = wo11 / SCG)));
            list.Add(string.Format(""));


            list.Add(string.Format("//Load of self weight of main long girder"));
            wo2 = SCG * BMG * DMG * Y_c_dry;
            list.Add(string.Format("wo2 = SCG*BMG*DMG*Y_c = {0:f3}*{1:f3}*{2:f3}*{3:f3} = {4:f3} kN.",
                SCG, BMG, DMG, Y_c_dry, wo2));

            wo2 = wo2 / 10;
            list.Add(string.Format("   = {0:f3} T.", wo2));
            list.Add(string.Format(""));
            list.Add(string.Format("load on main outer girder = wo1 / {0:f3} = {1:f3}/{0:f3} = {2:f3} Ton/m", SCG, wo2, (wo2 = wo2 / SCG)));
            list.Add(string.Format(""));
            
            list.Add(string.Format(""));
            list.Add(string.Format("//Load of one side Parapet wall"));
            wo3 = SCG * Hc * Wc * Y_c_dry;
            list.Add(string.Format("wo3 = SCG*Hp*Wp*Y_c = {0:f3}*{1:f3}*{2:f3}*{3:f3} = {4:f3} kN.",
               SCG, Hc, Wc, Y_c_dry, wo3));
            list.Add(string.Format(""));
            wo3 = wo3 / 10;
            list.Add(string.Format("   = {0:f3} T.", wo3));
            list.Add(string.Format(""));
            list.Add(string.Format("load on main outer girder = wo1 / {0:f3} = {1:f3}/{0:f3} = {2:f3} Ton", SCG, wo3, (wo3 = wo3 / SCG)));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Load of Side walk")); 
            
            wo4 = SCG * Wf * Hs * Y_c_dry;
            list.Add(string.Format("wo4 = SCG*Bs*Hs*Y_c = {0:f3}*{1:f3}*{2:f3}*{3:f3} = {4:f3} kN.",
                SCG, Wf, Hs, Y_c_dry, wo4));
            list.Add(string.Format(""));

            list.Add(string.Format(""));
            wo4 = wo4 / 10;
            list.Add(string.Format("   = {0:f3} T.", wo4));
            list.Add(string.Format(""));
            list.Add(string.Format("load on main outer girder = wo1 / {0:f3} = {1:f3}/{0:f3} = {1:f3} Ton/m",SCG, wo4, (wo4 = wo4 / SCG)));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Load of Side Walk Parapet wall")); wo5 = SCG * Wr * Wk * Y_c_dry;
            list.Add(string.Format("wo5 = SCG*Hps*Wps*Y_c = {0:f3}*{1:f3}*{2:f3}*{3:f3} = {4:f3} kN.",
                SCG, Wr, Wk, Y_c_dry, wo5));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format(""));

            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//In Analysis Input Data file UDL in all inner Girder members is to be mentioned as"));
            list.Add(string.Format(""));
            list.Add(string.Format("                **********************************"));
            list.Add(string.Format("                MEMBER LOAD "));


            long_member_load.Add(string.Format("LOAD 1 DEAD LOAD SELF WEIGHT"));
            long_member_load.Add(string.Format("MEMBER LOAD"));
            long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", inner_girders, wi1));
            long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", outer_girders, wo1));

            long_member_load.Add(string.Format("LOAD 2 DEAD LOAD DECK SLAB WET CONCRETE AND SHUTTERING"));
            long_member_load.Add(string.Format("MEMBER LOAD"));
            long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", inner_girders, wi2 / 2));
            long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", outer_girders, wo2 / 2));
            

            

            long_member_load.Add(string.Format("LOAD 3 DEAD LOAD DESHUTTERING"));
            long_member_load.Add(string.Format("MEMBER LOAD"));
            long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", inner_girders, wi2));
            long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", outer_girders, wo2));

            long_member_load.Add(string.Format("LOAD 4 DEAD LOAD SELF WEIGHT + DECK SLAB DRY CONCRETE"));
            long_member_load.Add(string.Format("MEMBER LOAD"));
            long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", inner_girders, (wi1 + wi2)));
            long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", outer_girders, (wo1 + wo2)));




            //Chiranjit [2013 10 05]
            //Dead Load Value for DeckSlab analysis
            #region Dead Load Value for DeckSlab analysis
            deck_member_load.Clear();
            deck_member_load.Add(wo3);
            deck_member_load.Add(wo2);
            deck_member_load.Add(wo11);


            #endregion Dead Load Value for DeckSlab analysis


            long_member_load.Add(string.Format("LOAD 5 SIDL CRASH BARRIER"));
            long_member_load.Add(string.Format("MEMBER LOAD"));
            long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", outer_girders, wo3));


            long_member_load.Add(string.Format("LOAD 6 SIDL WEARING COAT"));
            long_member_load.Add(string.Format("MEMBER LOAD"));
            long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", outer_girders, wo11));
            list.Add(string.Format(""));
            list.Add(string.Format(""));

            list.AddRange(long_member_load.ToArray());
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Concentrated JOINT LOADS in all main long Inner and Outer Girder members"));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Self weight of Cross Girders"));
            list.Add(string.Format("")); wc1 = SMG * DCG * BCG * Y_c_dry;
            list.Add(string.Format("wc1 = SMG*DCG*BCG*Y_c = {0:f3}*{1:f3}*{2:f3}*{3:f3} = {4:f3} kN.",
                SMG, DCG, BCG, Y_c_dry, wc1));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Total number of Inner Joints"));
            list.Add(string.Format("")); NIGJ = NMG * NCG;
            list.Add(string.Format("NIGJ = NMG*NCG = {0:f0}*{1:f0}= {2:f0} nos.", NMG, NCG, NIGJ));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Total number of Inner Joints in model (Constant value, always)"));
            list.Add(string.Format("")); NIMJ = 81;
            list.Add(string.Format("NIMJ = 81"));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Joint Loads applicable to all inner joints in model"));
            list.Add(string.Format("")); wjl = wc1 * NIGJ / NIMJ;
            list.Add(string.Format("wjl = wc1*NIGJ/NIMJ = {0:f3}*{1}/{2} = {3:f3} kN. = {4:f3} Ton.",
                 wc1, NIGJ, NIMJ, wjl, (wjl = wjl / 10.0)));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Factored Joint Load"));
            list.Add(string.Format("wjl = wjl*swf = {0:f3}*{1:f3} = {2:f4} Ton.",
                wjl, swf, (wjl = wjl * swf)));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//In Analysis Input Data file UDL in all inner Girder members is to be mentioned as"));
            list.Add(string.Format(""));
            list.Add(string.Format("                ***********************************"));
            list.Add(string.Format("                JOINT LOAD"));
            long_member_load.Add(string.Format("JOINT LOAD"));
            foreach (var item in joints_nos)
            {
                list.Add(string.Format("                {0} FY -{1:f4}", item, wjl));
                long_member_load.Add(string.Format("{0} FY -{1:f4}", item, wjl));

            }

            list.Add(string.Format(""));
            list.Add(string.Format("                ***********************************"));
            list.Add(string.Format(""));
            list.Add(string.Format("//END OF LOAD COMPUTATION"));
            list.Add(string.Format(""));



            //member_load.Add(string.Format(""));
            txt_member_load.Lines = long_member_load.ToArray();
            rtb_calc_load.Lines = list.ToArray();
            File.WriteAllLines(Path.Combine(user_path, "Load_Computation.txt"), list.ToArray());
            //iApp.RunExe(Path.Combine(user_path, "Load_Computation.txt"));
        }

        public void Calculate_Load_Computation(string outer_girders, string inner_girders, List<string> joints_nos)
        {
            if (Minor_Analysis._Outer_Girder_Support == null)
            {
                Ana_Initialize_Analysis_InputData();
                Minor_Analysis.CreateData();
                Minor_Analysis.WriteData_Total_Analysis(Minor_Analysis.Input_File, iApp.DesignStandard == eDesignStandard.BritishStandard);
            }
            outer_girders = Minor_Analysis._Outer_Girder_Support + " " + Minor_Analysis._Outer_Girder_Mid;
            inner_girders = Minor_Analysis._Inner_Girder_Support + " " + Minor_Analysis._Inner_Girder_Mid;
            //Long_Girder_Analysis._
            List<string> list = new List<string>();
            List<string> long_member_load = new List<string>();

            //Long_Girder_Analysis

            double SMG, SCG, wi1, wi2, wi3, wi4, wi5, wi6, NIG, NIM, wiu, wo1, wo2, wo3, wo4, wo5, wo6, wo7, NOG, NOM;
            double wou, wc1, NIGJ, NIMJ, wjl, C;

            list.Add(string.Format(""));
            list.Add(string.Format("--------------------------------------------------------------------"));
            list.Add(string.Format("ASTRA Load Computation for PSC T - Girder Bridge"));
            list.Add(string.Format("--------------------------------------------------------------------"));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Spacing of main long girders "));
            SMG = (B - CL - CR) / (NMG - 1);
            list.Add(string.Format("SMG = (B-CL-CR)/(NMG-1) = ({0:f3}-{1:f3}-{2:f3})/({3:f0}-1) = {4:f3} m.",
                B, CL, CR, NMG, SMG));
            list.Add(string.Format(""));
            list.Add(string.Format("//Spacing of cross girders "));
            SCG = L / (NCG - 1);
            list.Add(string.Format("SCG = L/(NCG-1) = {0:f3}/({1:f0}-1) = {2:f3} m.",
                L, NCG, SCG));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//UDL in all main long Inner Girder members"));
            list.Add(string.Format(""));

            //Self weight
            wi1 = (LG_INNER_MID.Girder_Section_A) * Y_c_dry / 10;
            wo1 = (LG_OUTER_MID.Girder_Section_A) * Y_c_dry / 10;


            long_member_load.Add(string.Format("LOAD 1 DEAD LOAD SELF WEIGHT"));
            long_member_load.Add(string.Format("MEMBER LOAD"));
            if (Minor_Analysis._Inner_Girder_Mid.StartsWith("0") == false)
                long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", Minor_Analysis._Inner_Girder_Mid, wi1));
            long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", Minor_Analysis._Outer_Girder_Mid, wo1));


            wi1 = (LG_INNER_SUP.Girder_Section_A) * Y_c_dry / 10;
            wo1 = (LG_OUTER_SUP.Girder_Section_A) * Y_c_dry / 10;


            if (Minor_Analysis._Inner_Girder_Support.StartsWith("0") == false)
                long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", Minor_Analysis._Inner_Girder_Support, wi1));
            long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", Minor_Analysis._Outer_Girder_Support, wo1));



            //Deck Slab Load Dry Concrete
            wi2 = (SMG / 2) * Ds * Y_c_dry / 10;
            wo2 = (CL + SMG / 2) * Ds * Y_c_dry / 10;


            //Deck Slab Load Wet Concrete
            wi3 = (SMG / 2) * Ds * Y_c_wet / 10;
            wo3 = (CL + SMG / 2) * Ds * Y_c_wet / 10;

            //Shuttering
            wi4 = (SMG / 2) * Ds * ils / 10;
            wo4 = (CL + SMG / 2) * Ds * ils / 10;

            //Crash Barrier
            //wi5 = wgc;
            wo5 = wgc / 10;


            // Wearing Coat
            //wi6 = (SMG / 2) * Ds * ils;
            wo6 = SCG * Dw * Y_w / 10;


            long_member_load.Add(string.Format("LOAD 2 DEAD LOAD DECK SLAB WET CONCRETE AND SHUTTERING"));
            long_member_load.Add(string.Format("MEMBER LOAD"));
            if (inner_girders.StartsWith("0") == false)
                long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", inner_girders, (wi3 - wi4)));
            long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", outer_girders, (wo3 - wo4)));

            long_member_load.Add(string.Format("LOAD 3 DEAD LOAD DESHUTTERING"));
            long_member_load.Add(string.Format("MEMBER LOAD"));
            if (inner_girders.StartsWith("0") == false)
                long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", inner_girders, (wi3 + wi4)));
            long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", outer_girders, (wo3 + wo4)));

            long_member_load.Add(string.Format("LOAD 4 DEAD LOAD SELF WEIGHT + DECK SLAB DRY CONCRETE"));
            long_member_load.Add(string.Format("MEMBER LOAD"));
            if (inner_girders.StartsWith("0") == false)
                long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", inner_girders, (wi1 + wi2)));
            long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", outer_girders, (wo1 + wo2)));


            long_member_load.Add(string.Format("LOAD 5 SIDL CRASH BARRIER"));
            long_member_load.Add(string.Format("MEMBER LOAD"));
            long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", outer_girders, wo5));


            long_member_load.Add(string.Format("LOAD 6 SIDL WEARING COAT"));
            long_member_load.Add(string.Format("MEMBER LOAD"));
            long_member_load.Add(string.Format("{0} UNI GY -{1:f4}", outer_girders, wo6));



            #region Dead Load Value for DeckSlab analysis
            deck_member_load.Clear();
            deck_member_load.Add(wo3);
            deck_member_load.Add(wo2);
            deck_member_load.Add(wo3);
            #endregion Dead Load Value for DeckSlab analysis



            list.Add(string.Format(""));
            list.Add(string.Format(""));

            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Concentrated JOINT LOADS in all main long Inner and Outer Girder members"));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Self weight of Cross Girders"));
            list.Add(string.Format("")); wc1 = SMG * DCG * BCG * Y_c_dry;
            list.Add(string.Format("wc1 = SMG*DCG*BCG*Y_c = {0:f3}*{1:f3}*{2:f3}*{3:f3} = {4:f3} kN.",
                SMG, DCG, BCG, Y_c_dry, wc1));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Total number of Inner Joints"));
            list.Add(string.Format("")); NIGJ = NMG * NCG;
            list.Add(string.Format("NIGJ = NMG*NCG = {0:f0}*{1:f0}= {2:f0} nos.", NMG, NCG, NIGJ));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Total number of Inner Joints in model (Constant value, always)"));
            list.Add(string.Format("")); NIMJ = 81;
            list.Add(string.Format("NIMJ = 81"));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Joint Loads applicable to all inner joints in model"));
            list.Add(string.Format("")); wjl = wc1 * NIGJ / NIMJ;
            list.Add(string.Format("wjl = wc1*NIGJ/NIMJ = {0:f3}*{1}/{2} = {3:f3} kN. = {4:f3} Ton.",
                 wc1, NIGJ, NIMJ, wjl, (wjl = wjl / 10.0)));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//Factored Joint Load"));
            list.Add(string.Format("wjl = wjl*swf = {0:f3}*{1:f3} = {2:f4} Ton.",
                wjl, swf, (wjl = wjl * swf)));
            list.Add(string.Format(""));
            list.Add(string.Format(""));
            list.Add(string.Format("//In Analysis Input Data file UDL in all inner Girder members is to be mentioned as"));
            list.Add(string.Format(""));
            list.Add(string.Format("                ***********************************"));
            list.Add(string.Format("                JOINT LOAD"));
            long_member_load.Add(string.Format("JOINT LOAD"));
            foreach (var item in joints_nos)
            {
                list.Add(string.Format("                {0} FY -{1:f4}", item, wjl));
                long_member_load.Add(string.Format("{0} FY -{1:f4}", item, wjl));

            }

            list.Add(string.Format(""));
            list.Add(string.Format("                ***********************************"));
            list.Add(string.Format(""));
            list.Add(string.Format("//END OF LOAD COMPUTATION"));
            list.Add(string.Format(""));



            //member_load.Add(string.Format(""));
            txt_member_load.Lines = long_member_load.ToArray();
            rtb_calc_load.Lines = list.ToArray();
            if(Path.GetFileName(user_path) == Project_Name)
            File.WriteAllLines(Path.Combine(user_path, "Load_Computation.txt"), list.ToArray());
            //iApp.RunExe(Path.Combine(user_path, "Load_Computation.txt"));
        }
       
        #endregion Chiranjit [2012 06 10]

        #region Chiranjit [2012 07 06]

        #region View Force
        string DL_Analysis_Rep = "";
        string LL_Analysis_Rep = "";

        SupportReactionTable DL_support_reactions = null;
        SupportReactionTable LL_support_reactions = null;
        string Supports = "";
        //IApplication iApp = null;
        //double B = 0.0;
        public void frm_ViewForces(double abut_width, string DL_Analysis_Report_file, string LL_Analysis_Report_file, string supports)
        {
            //iApp = app;
            DL_Analysis_Rep = DL_Analysis_Report_file;
            LL_Analysis_Rep = LL_Analysis_Report_file;
            Supports = supports.Replace(",", " ");
            //B = abut_width;
        }
        public string Total_DeadLoad_Reaction
        {
            get
            {
                return txt_dead_kN_m.Text;
            }
            set
            {
                txt_dead_kN_m.Text = value;
            }
        }
        public string Total_LiveLoad_Reaction
        {
            get
            {
                return txt_live_kN_m.Text;
            }
            set
            {
                txt_live_kN_m.Text = value;
            }
        }
        void frm_ViewForces_Load()
        {
            try
            {
                DL_support_reactions = new SupportReactionTable(iApp, DL_Analysis_Rep);
                LL_support_reactions = new SupportReactionTable(iApp, LL_Analysis_Rep);
                Show_and_Save_Data_DeadLoad();
            }
            catch (Exception ex) { }
        }
        void Show_and_Save_Data_DeadLoad()
        {

            dgv_left_end_design_forces.Rows.Clear();
            dgv_right_end_design_forces.Rows.Clear();

            SupportReaction sr = null;
            MyList mlist = new MyList(MyList.RemoveAllSpaces(Supports), ' ');

            double tot_dead_vert_reac = 0.0;
            double tot_live_vert_reac = 0.0;

            for (int i = 0; i < mlist.Count; i++)
            {
                try
                {
                    sr = DL_support_reactions.Get_Data(mlist.GetInt(i));
                    dgv_left_end_design_forces.Rows.Add(sr.JointNo, Math.Abs(sr.Max_Reaction).ToString("f3"));

                    tot_dead_vert_reac += Math.Abs(sr.Max_Reaction); ;
                }
                catch (Exception ex)
                {
                }

            }

            for (int i = 0; i < mlist.Count; i++)
            {
                try
                {
                    sr = LL_support_reactions.Get_Data(mlist.GetInt(i));
                    dgv_right_end_design_forces.Rows.Add(sr.JointNo, Math.Abs(sr.Max_Reaction).ToString("f3"));
                    tot_live_vert_reac += Math.Abs(sr.Max_Reaction);
                }
                catch (Exception ex)
                {
                }

            }

            txt_dead_vert_reac_ton.Text = (tot_dead_vert_reac).ToString("f3");
            txt_live_vert_rec_Ton.Text = (tot_live_vert_reac).ToString("f3");
        }


        private void txt_dead_vert_reac_ton_TextChanged(object sender, EventArgs e)
        {
            TextBox txt = sender as TextBox;

            //if (txt.Name == txt_dead_vert_reac_ton.Name)
            //{
            Text_Changed_Forces();
            //}

        }

        private void Text_Changed_Forces()
        {
            if (B != 0)
            {
                txt_dead_vert_reac_kN.Text = ((MyList.StringToDouble(txt_dead_vert_reac_ton.Text, 0.0) * 10)).ToString("f3");
                txt_dead_kN_m.Text = ((MyList.StringToDouble(txt_dead_vert_reac_ton.Text, 0.0) * 10) / B).ToString("f3");
                //}
                //else if (txt.Name == txt_live_vert_rec_Ton.Name)
                //{
                txt_live_vert_rec_kN.Text = ((MyList.StringToDouble(txt_live_vert_rec_Ton.Text, 0.0) * 10)).ToString("f3");
                txt_live_kN_m.Text = ((MyList.StringToDouble(txt_live_vert_rec_Ton.Text, 0.0) * 10) / B).ToString("f3");
                //}
            }
            //else if (txt.Name == txt_dead_kN_m.Name)
            //{
            //txt_abut_w5.Text = txt_dead_kN_m.Text;
            txt_pier_2_P2.Text = txt_dead_kN_m.Text;
            //}
            //else if (txt.Name == txt_live_kN_m.Name)
            //{
            //txt_abut_w6.Text = txt_live_kN_m.Text;
            txt_pier_2_P3.Text = txt_live_kN_m.Text;
            //}
            //else if (txt.Name == txt_final_vert_rec_kN.Name)
            //{
            txt_RCC_Pier_W1_supp_reac.Text = txt_final_vert_rec_kN.Text;
            //}
            //else if (txt.Name == txt_max_Mx_kN.Name)
            //{
            txt_RCC_Pier_Mx1.Text = txt_max_Mx_kN.Text;
            //}
            //else if (txt.Name == txt_max_Mz_kN.Name)
            //{
            txt_RCC_Pier_Mz1.Text = txt_max_Mz_kN.Text;


            //txt_abut_B.Text = txt_RCC_Pier__B.Text = txt_RCC_Pier___B.Text = txt_Ana_B.Text;

            //txt_RCC_Pier_L.Text = txt_abut_L.Text = txt_Ana_L.Text;



        }
        #endregion View Force

        #region frm_Pier_ViewDesign_Forces
        string analysis_rep = "";
        SupportReactionTable support_reactions = null;
        string Left_support = "";
        string Right_support = "";
        public void frm_Pier_ViewDesign_Forces(string Analysis_Report_file, string left_support, string right_support)
        {

            analysis_rep = Analysis_Report_file;

            Left_support = left_support.Replace(",", " ");
            Right_support = right_support.Replace(",", " ");
        }

        private void frm_ViewDesign_Forces_Load()
        {
            support_reactions = new SupportReactionTable(iApp, analysis_rep);
            try
            {
                Show_and_Save_Data();
            }
            catch (Exception ex) { }
        }

        void Show_and_Save_Data()
        {
            if (!File.Exists(analysis_rep)) return;
            string format = "{0,27} {1,10:f3} {2,10:f3} {3,10:f3}";
            List<string> list_arr = new List<string>(File.ReadAllLines(analysis_rep));
            list_arr.Add("");
            list_arr.Add("                   =====================================");
            list_arr.Add("                     DESIGN FORCES FOR RCC PIER DESIGN");
            list_arr.Add("                   =====================================");
            list_arr.Add("");
            list_arr.Add("");
            list_arr.Add(string.Format(""));
            list_arr.Add(string.Format(format, "JOINT", "VERTICAL", "MAXIMUM", "MAXIMUM"));
            list_arr.Add(string.Format(format, "NOS", "REACTIONS", "MX", "MZ"));
            list_arr.Add(string.Format(format, "   ", "  (Ton)   ", "  (Ton-m)", "  (Ton-m)"));
            list_arr.Add("");
            SupportReaction sr = null;

            MyList mlist = new MyList(MyList.RemoveAllSpaces(Left_support), ' ');

            double tot_left_vert_reac = 0.0;
            double tot_right_vert_reac = 0.0;

            double tot_left_Mx = 0.0;
            double tot_left_Mz = 0.0;

            double tot_right_Mx = 0.0;
            double tot_right_Mz = 0.0;


            dgv_left_des_frc.Rows.Clear();
            dgv_right_des_frc.Rows.Clear();
            list_arr.Add("LEFT END");
            list_arr.Add("--------");
            for (int i = 0; i < mlist.Count; i++)
            {
                sr = support_reactions.Get_Data(mlist.GetInt(i));
                dgv_left_des_frc.Rows.Add(sr.JointNo, sr.Max_Reaction, sr.Max_Mx, sr.Max_Mz);

                tot_left_vert_reac += Math.Abs(sr.Max_Reaction); ;
                tot_left_Mx += sr.Max_Mx;
                tot_left_Mz += sr.Max_Mz;
                list_arr.Add(string.Format(format, sr.JointNo, Math.Abs(sr.Max_Reaction), sr.Max_Mx, sr.Max_Mz));
            }

            list_arr.Add("");

            //Chiranjit [2012 07 06]
            //Change unit kN to Ton
            //tot_left_vert_reac /= 10.0;
            //tot_left_Mx /= 10.0;
            //tot_left_Mz /= 10.0;

            txt_left_total_vert_reac.Text = tot_left_vert_reac.ToString("0.000");
            txt_left_total_Mx.Text = tot_left_Mx.ToString("0.000");
            txt_left_total_Mz.Text = tot_left_Mz.ToString("0.000");
            list_arr.Add(string.Format(format, "TOTAL", tot_left_vert_reac, tot_left_Mx, tot_left_Mz));
            list_arr.Add("");

            mlist = new MyList(MyList.RemoveAllSpaces(Right_support), ' ');
            list_arr.Add("RIGHT END");
            list_arr.Add("--------");
            for (int i = 0; i < mlist.Count; i++)
            {
                sr = support_reactions.Get_Data(mlist.GetInt(i));
                dgv_right_des_frc.Rows.Add(sr.JointNo, Math.Abs(sr.Max_Reaction), sr.Max_Mx, sr.Max_Mz);

                tot_right_vert_reac += Math.Abs(sr.Max_Reaction);
                tot_right_Mx += sr.Max_Mx;
                tot_right_Mz += sr.Max_Mz;
                list_arr.Add(string.Format(format, sr.JointNo, Math.Abs(sr.Max_Reaction), sr.Max_Mx, sr.Max_Mz));
            }
            list_arr.Add("");

            //Chiranjit [2012 07 06]
            //Change unit kN to Ton
            //tot_right_vert_reac /= 10.0;
            //tot_right_Mx /= 10.0;
            //tot_right_Mz /= 10.0;
            txt_right_total_vert_reac.Text = tot_right_vert_reac.ToString("0.000");
            txt_right_total_Mx.Text = tot_right_Mx.ToString("0.000");
            txt_right_total_Mz.Text = tot_right_Mz.ToString("0.000");
            list_arr.Add("");


            list_arr.Add(string.Format(format, "TOTAL", tot_right_vert_reac, tot_right_Mx, tot_right_Mz));
            list_arr.Add("");


            //txt_both_ends_total.Text = (tot_left_vert_reac + tot_right_vert_reac).ToString("0.000");
            list_arr.Add("");
            //list_arr.Add("BOTH ENDS TOTAL VERTICAL REACTION = " + txt_both_ends_total.Text + " Ton");

            txt_final_vert_reac.Text = (tot_right_vert_reac + tot_left_vert_reac).ToString("0.000");
            txt_final_vert_rec_kN.Text = ((tot_right_vert_reac + tot_left_vert_reac) * 10).ToString("0.000");


            list_arr.Add("");
            list_arr.Add("");
            list_arr.Add("FINAL DESIGN FORCES");
            list_arr.Add("-------------------");
            list_arr.Add("");
            list_arr.Add("TOTAL VERTICAL REACTION = " + txt_final_vert_reac.Text + " Ton" + "    =  " + txt_final_vert_rec_kN.Text + " kN");

            txt_final_Mx.Text = ((tot_left_Mx > tot_right_Mx) ? tot_left_Mx : tot_right_Mx).ToString("0.000");
            txt_max_Mx_kN.Text = (MyList.StringToDouble(txt_final_Mx.Text, 0.0) * 10.0).ToString("f3");


            list_arr.Add("        MAXIMUM  MX     = " + txt_final_Mx.Text + " Ton-M" + "  =  " + txt_max_Mx_kN.Text + " kN-m");
            txt_final_Mz.Text = ((tot_left_Mz > tot_right_Mz) ? tot_left_Mz : tot_right_Mz).ToString("0.000");
            txt_max_Mz_kN.Text = (MyList.StringToDouble(txt_final_Mz.Text, 0.0) * 10.0).ToString("f3");

            list_arr.Add("        MAXIMUM  MZ     = " + txt_final_Mz.Text + " Ton-M" + "  =  " + txt_max_Mz_kN.Text + " kN-m");
            list_arr.Add("");
            list_arr.Add("");
            list_arr.Add("                  ========================================");
            list_arr.Add("                  END OF DESIGN FORCES FOR RCC PIER DESIGN");
            list_arr.Add("                  ========================================");
            list_arr.Add("");





            File.WriteAllLines(analysis_rep, list_arr.ToArray());

            list_arr.Clear();
            list_arr.Add("W1=" + txt_final_vert_rec_kN.Text);
            list_arr.Add("Mx1=" + txt_max_Mx_kN.Text);
            list_arr.Add("Mz1=" + txt_max_Mz_kN.Text);
            string f_path = Path.Combine(Path.GetDirectoryName(analysis_rep), "Forces.fil");
            File.WriteAllLines(f_path, list_arr.ToArray());
            Environment.SetEnvironmentVariable("PIER", f_path);
        }
        #endregion frm_Pier_ViewDesign_Forces

        #endregion

        #region Chiranjit [2012 07 10]
        //Write All Data in a File
        public void Write_All_Data()
        {
            Write_All_Data(true);
        }
        public void Write_All_Data(bool showMessage)
        {
            Text_Changed();

            if (showMessage) DemoCheck();

            iApp.Save_Form_Record(this, user_path);
        }
        public void Read_All_Data()
        {
            //if (iApp.IsDemo) return;

            //string data_file = Long_Girder_Analysis.User_Input_Data;

            //if (!File.Exists(data_file)) return;
            try
            {
                Abut.FilePath = user_path;
                rcc_pier.FilePath = user_path;
                if(iApp.DesignStandard == eDesignStandard.IndianStandard)
                {

                    uC_Deckslab_IS1.iApp = iApp;
                    uC_Deckslab_IS1.user_path = user_path;

                    uC_Deckslab_IS1.deck_member_load = deck_member_load;
                    uC_Deckslab_IS1.L = L;
                    uC_Deckslab_IS1.NMG = NMG;
                    uC_Deckslab_IS1.SMG = SMG;
                    //uC_Deckslab_IS1.os = os;
                    uC_Deckslab_IS1.CL = CL;
                    uC_Deckslab_IS1.Ds = Ds;
                    uC_Deckslab_IS1.B = B;


                    uC_Deckslab_IS1.Width_LeftCantilever = MyList.StringToDouble(txt_Ana_CL.Text, 0.0);
                    uC_Deckslab_IS1.Width_RightCantilever = MyList.StringToDouble(txt_Ana_CR.Text, 0.0);
                    //uC_Deckslab_IS1.Skew_Angle = MyList.StringToDouble(dgv_deck_user_input[1, 5].Value.ToString(), 0.0);
                    uC_Deckslab_IS1.Number_Of_Long_Girder = MyList.StringToInt(txt_Ana_NMG.Text, 4);
                    uC_Deckslab_IS1.Number_Of_Cross_Girder = MyList.StringToInt(txt_Ana_NCG.Text, 3);
                    uC_Deckslab_IS1.WidthBridge = L / (NCG - 1);
                }
            }
            catch (Exception ex) { }
            Button_Enable_Disable();
        }
        #endregion Chiranjit [2012 07 10]

        #region Chiranjit [2012 07 20]

        private void DemoCheck()
        {
            if (iApp.Check_Demo_Version())
            {
                txt_Ana_L.Text = "0.0";
                txt_Ana_L.Text = "19.58";
                txt_Ana_B.Text = "12.0";
                cmb_NMG.Text = "4";
                txt_Ana_CW.Text = "11.0";

                Deckslab_User_Input();
                Long_Girder_User_Input();
                Cross_Girder_User_Input();
            }
        }
        #endregion Chiranjit [2012 07 20]

       
        #region Excel Files

        public string Excel_Long_Girder
        {
            get
            {

                if (iApp.DesignStandard == eDesignStandard.BritishStandard)
                    return Path.Combine(Application.StartupPath, @"DESIGN\Limit State Method\RCC T Girder BS\RCC Precast Long Girder BS.xlsm");
                //Indian Standard
                return Path.Combine(Application.StartupPath, @"DESIGN\Limit State Method\RCC T Girder IS\RCC Precast Long Girder IS.xlsm");
            }
        }
        public string Excel_Cross_Girder
        {
            get
            {
                if (iApp.DesignStandard == eDesignStandard.BritishStandard)
                    return Path.Combine(Application.StartupPath, @"DESIGN\Limit State Method\RCC T Girder BS\RCC Cross Girder BS.xlsx");

                return Path.Combine(Application.StartupPath, @"DESIGN\Limit State Method\RCC T Girder IS\RCC Cross Girder IS.xlsx");
            }
        }
        public string Excel_Deckslab
        {
            get
            {
                if (iApp.DesignStandard == eDesignStandard.BritishStandard)
                    return Path.Combine(Application.StartupPath, @"DESIGN\Limit State Method\RCC T Girder BS\RCC Deck Slab BS.xls");

                return Path.Combine(Application.StartupPath, @"DESIGN\Limit State Method\RCC T Girder IS\RCC Deck Slab IS.xlsx");
            }
        }

        #endregion Excel Files
        private void btn_LS_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            string excel_file_name = "";
            string copy_path = "";
            if (btn.Name == btn_LS_long_ws.Name)
            {
                //excel_file_name = Path.Combine(Application.StartupPath, @"DESIGN\Limit State Method\RCC T Girder LS\RCC Precast Long Girder.xlsm");


                //if (iApp.DesignStandard == eDesignStandard.BritishStandard)
                //    excel_file_name = Path.Combine(Application.StartupPath, @"DESIGN\Limit State Method\RCC T Girder BS\RCC Precast Long Girder BS.xlsm");
                //else
                //    excel_file_name = Path.Combine(Application.StartupPath, @"DESIGN\Limit State Method\RCC T Girder IS\RCC Precast Long Girder IS.xlsm");
                Write_All_Data(true);

                excel_file_name = Excel_Long_Girder;

                if (!File.Exists(excel_file_name))
                {
                    MessageBox.Show("Excel Program Module not found in Application folder.\n\n" + excel_file_name, "ASTRA", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    return;
                }
                
                copy_path = Path.Combine(Worksheet_Folder, Path.GetFileName(excel_file_name));
                File.Copy(excel_file_name, copy_path, true);
                RCC_T_Girder_Excel_Update rcc_excel = new RCC_T_Girder_Excel_Update();
                rcc_excel.Excel_File_Name = copy_path;
                rcc_excel.Long_User_Inputs.Read_From_Grid(dgv_long_user_input);
                rcc_excel.Report_File_Name = File_Long_Girder_Results;
                iApp.Excel_Open_Message();
                rcc_excel.Update_Excel_Long_Girder();
                Button_Enable_Disable();
                return;

            }
            else if (btn.Name == btn_LS_long_rep_open.Name)
            {
                copy_path = Path.Combine(Worksheet_Folder, Path.GetFileName(Excel_Long_Girder));
                if (File.Exists(copy_path))
                    iApp.OpenExcelFile(copy_path, "2011ap");
            }
            else if (btn.Name == btn_LS_cross_rep_open.Name)
            {
                copy_path = Path.Combine(Worksheet_Folder, Path.GetFileName(Excel_Cross_Girder));
                if (File.Exists(copy_path))
                    iApp.OpenExcelFile(copy_path, "2011ap");

            }
            //else if (btn.Name == btn_LS_deck_rep_open.Name)
            //{
            //    copy_path = Path.Combine(Worksheet_Folder, Path.GetFileName(Excel_Deckslab));
            //    if (File.Exists(copy_path))
            //        iApp.OpenExcelFile(copy_path, "2011ap");
            //}
            else if (btn.Name == btn_LS_cross_ws.Name)
            {
                //if (iApp.DesignStandard == eDesignStandard.BritishStandard)
                //    excel_file_name = Path.Combine(Application.StartupPath, @"DESIGN\Limit State Method\RCC T Girder BS\RCC Cross Girder BS.xls");
                //else
                //    excel_file_name = Path.Combine(Application.StartupPath, @"DESIGN\Limit State Method\RCC T Girder IS\RCC Cross Girder IS.xls");

                Write_All_Data(true);

                excel_file_name = Excel_Cross_Girder;


                if (!File.Exists(excel_file_name))
                {
                    MessageBox.Show("Excel Program Module not found in Application folder.\n\n" + excel_file_name, "ASTRA", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    return;
                }

                copy_path = Path.Combine(Worksheet_Folder, Path.GetFileName(excel_file_name));
                File.Copy(excel_file_name, copy_path, true);
                RCC_T_Girder_Excel_Update rcc_excel = new RCC_T_Girder_Excel_Update();
                rcc_excel.Excel_File_Name = copy_path;
                rcc_excel.Cross_User_Inputs.Read_From_Grid(dgv_cross_user_input);
                iApp.Excel_Open_Message();
                rcc_excel.Insert_Values_into_Excel_Cross_Girder();
                Button_Enable_Disable();
                return;

            }
            else
            {
                iApp.Open_WorkSheet_Design();
                return;
            }

            copy_path = Path.Combine(user_path, Path.GetFileName(excel_file_name));

            if (File.Exists(excel_file_name))
            {
                iApp.OpenExcelFile(Worksheet_Folder, excel_file_name, "2011ap");
            }
            Button_Enable_Disable();
        }

        private void btn_Deck_Analysis_Click(object sender, EventArgs e)
        {
            //Write_All_Data(true);
            user_path = IsCreateData ? Path.Combine(iApp.LastDesignWorkingFolder, Title) : user_path;
            if (!Directory.Exists(user_path))
            {
                Directory.CreateDirectory(user_path);
            }
            try
            {
                DECKSLAB_LL_TXT();
                #region Process
                int i = 1;
                Write_All_Data(true);

                string flPath = Deck_Analysis.Input_File;

                ProcessCollection pcol = new ProcessCollection();

                ProcessData pd = new ProcessData();

                do
                {
                    if (i == 1)
                    {
                        flPath = Deck_Analysis.LL_Analysis_1_Input_File;
                    }
                    else if (i == 2)
                    {
                        flPath = Deck_Analysis.LL_Analysis_2_Input_File;
                    }
                    else if (i == 3)
                    {
                        flPath = Deck_Analysis.LL_Analysis_3_Input_File;
                    }
                    else if (i == 4)
                    {

                        flPath = Deck_Analysis.LL_Analysis_4_Input_File;
                    }
                    else if (i == 5)
                    {

                        flPath = Deck_Analysis.LL_Analysis_5_Input_File;
                    }
                    else if (i == 6)
                    {
                        flPath = Deck_Analysis.LL_Analysis_6_Input_File;
                    }
                    else if (i == 7)
                    {
                        flPath = Deck_Analysis.DeadLoadAnalysis_Input_File;
                    }


                    //MessageBox.Show(this, "PROCESS ANALYSIS FOR "
                    // + Path.GetFileNameWithoutExtension(flPath).ToUpper(), "ASTRA", MessageBoxButtons.OK, MessageBoxIcon.Information);



                    //File.WriteAllText(Path.Combine(Path.GetDirectoryName(flPath), "PAT001.tmp"), Path.GetDirectoryName(flPath) + "\\");
                    //File.WriteAllText(Path.Combine(iApp.AppFolder, "PAT001.tmp"), Path.GetDirectoryName(flPath) + "\\");
                    ////System.Environment.SetEnvironmentVariable("SURVEY", flPath);

                    //System.Diagnostics.Process prs = new System.Diagnostics.Process();

                    //System.Environment.SetEnvironmentVariable("SURVEY", flPath);
                    //System.Environment.SetEnvironmentVariable("ASTRA", flPath);

                    //prs.StartInfo.FileName = Path.Combine(Application.StartupPath, "ast001.exe");
                    //if (prs.Start())
                    //    prs.WaitForExit();


                    pd = new ProcessData();
                    pd.Process_File_Name = flPath;
                    pd.Process_Text = "PROCESS ANALYSIS FOR " + Path.GetFileNameWithoutExtension(flPath).ToUpper();
                    pcol.Add(pd);


                    i++;
                }
                while (i <= 7);




                //frm_LS_Process ff = new frm_LS_Process(pcol);
                //ff.Owner = this;
                ////ff.ShowDialog();
                //if (ff.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;



                if (!iApp.Show_and_Run_Process_List(pcol))
                {
                    iApp.Progress_Works.Clear();
                    return;
                }

                //while (i < 3) ;

                //string ana_rep_file = Bridge_Analysis.Analysis_Report;
                string ana_rep_file = Deck_Analysis.Get_Analysis_Report_File(Deck_Analysis.LL_Analysis_1_Input_File);


                iApp.Progress_Works.Clear();
                //iApp.Progress_Works.Add("Reading Analysis Data from Total Load Analysis Report File (ANALYSIS_REP.TXT)");
                //iApp.Progress_Works.Add("Set Structure Geometry for Total Load Analysis");
                //iApp.Progress_Works.Add("Reading Bending Moment & Shear Force from Analysis Result");


                //iApp.Progress_Works.Add("Reading Analysis Data from Live Load Analysis Report File (ANALYSIS_REP.TXT)");
                //iApp.Progress_Works.Add("Set Structure Geometry for Live Load Analysis");
                //iApp.Progress_Works.Add("Reading Bending Moment & Shear Force from Analysis Result");



                iApp.Progress_Works.Add("Reading Analysis Data from Live Load Analysis 1 Report File (ANALYSIS_REP.TXT)");
                //iApp.Progress_Works.Add("Set Structure Geometry for Live Load Analysis");
                //iApp.Progress_Works.Add("Reading Bending Moment & Shear Force from Analysis Result");



                iApp.Progress_Works.Add("Reading Analysis Data from Live Load Analysis 2 Report File (ANALYSIS_REP.TXT)");
                //iApp.Progress_Works.Add("Set Structure Geometry for Live Load Analysis");
                //iApp.Progress_Works.Add("Reading Bending Moment & Shear Force from Analysis Result");



                iApp.Progress_Works.Add("Reading Analysis Data from Live Load Analysis 3 Report File (ANALYSIS_REP.TXT)");
                //iApp.Progress_Works.Add("Set Structure Geometry for Live Load Analysis");
                //iApp.Progress_Works.Add("Reading Bending Moment & Shear Force from Analysis Result");



                iApp.Progress_Works.Add("Reading Analysis Data from Live Load Analysis 4 Report File (ANALYSIS_REP.TXT)");
                //iApp.Progress_Works.Add("Set Structure Geometry for Live Load Analysis");
                //iApp.Progress_Works.Add("Reading Bending Moment & Shear Force from Analysis Result");



                iApp.Progress_Works.Add("Reading Analysis Data from Live Load Analysis 5 Report File (ANALYSIS_REP.TXT)");
                //iApp.Progress_Works.Add("Set Structure Geometry for Live Load Analysis");
                //iApp.Progress_Works.Add("Reading Bending Moment & Shear Force from Analysis Result");



                iApp.Progress_Works.Add("Reading Analysis Data from Live Load Analysis 6 Report File (ANALYSIS_REP.TXT)");
                //iApp.Progress_Works.Add("Set Structure Geometry for Live Load Analysis");
                //iApp.Progress_Works.Add("Reading Bending Moment & Shear Force from Analysis Result");



                iApp.Progress_Works.Add("Reading Analysis Data from Dead Load Analysis Report File (ANALYSIS_REP.TXT)");

                //iApp.Progress_Works.Add("Reading support reaction forces from Total Load Analysis Report");
                //iApp.Progress_Works.Add("Reading support reaction forces from Live Load Analysis Report");
                //iApp.Progress_Works.Add("Reading support reaction forces from Dead Load Analysis Report");


                //Deck_Analysis.TotalLoad_Analysis = null;
                //Long_Girder_Analysis.TotalLoad_Analysis = new BridgeMemberAnalysis(iApp, Long_Girder_Analysis.Total_Analysis_Report);
                //Long_Girder_Analysis.LiveLoad_Analysis = new BridgeMemberAnalysis(iApp, Long_Girder_Analysis.LiveLoad_Analysis_Report);


                Deck_Analysis.LiveLoad_1_Analysis = new BridgeMemberAnalysis(iApp,
                    Deck_Analysis.Get_Analysis_Report_File(Deck_Analysis.LL_Analysis_1_Input_File));


                Deck_Analysis.LiveLoad_2_Analysis = new BridgeMemberAnalysis(iApp,
                    Deck_Analysis.Get_Analysis_Report_File(Deck_Analysis.LL_Analysis_2_Input_File));

                Deck_Analysis.LiveLoad_3_Analysis = new BridgeMemberAnalysis(iApp,
                    Deck_Analysis.Get_Analysis_Report_File(Deck_Analysis.LL_Analysis_3_Input_File));

                Deck_Analysis.LiveLoad_4_Analysis = new BridgeMemberAnalysis(iApp,
                    Deck_Analysis.Get_Analysis_Report_File(Deck_Analysis.LL_Analysis_4_Input_File));

                Deck_Analysis.LiveLoad_5_Analysis = new BridgeMemberAnalysis(iApp,
                    Deck_Analysis.Get_Analysis_Report_File(Deck_Analysis.LL_Analysis_5_Input_File));

                Deck_Analysis.LiveLoad_6_Analysis = new BridgeMemberAnalysis(iApp,
                    Deck_Analysis.Get_Analysis_Report_File(Deck_Analysis.LL_Analysis_6_Input_File));


                Deck_Analysis.DeadLoad_Analysis = new BridgeMemberAnalysis(iApp, Deck_Analysis.DeadLoad_Analysis_Report);


                if (!iApp.Is_Progress_Cancel)
                    Show_Deckslab_Moment_Shear();
                else
                {
                    iApp.Progress_Works.Clear();
                    iApp.Progress_OFF();
                    return;

                }

            //string s1 = "";
            //string s2 = "";
            //try
            //{
            //    for (i = 0; i < Long_Girder_Analysis.TotalLoad_Analysis.Supports.Count; i++)
            //    {
            //        if (i < Long_Girder_Analysis.TotalLoad_Analysis.Supports.Count / 2)
            //        {
            //            if (i == Long_Girder_Analysis.TotalLoad_Analysis.Supports.Count / 2 - 1)
            //            {
            //                s1 += Long_Girder_Analysis.TotalLoad_Analysis.Supports[i].NodeNo;
            //            }
            //            else
            //                s1 += Long_Girder_Analysis.TotalLoad_Analysis.Supports[i].NodeNo + ",";
            //        }
            //        else
            //        {
            //            if (i == Long_Girder_Analysis.TotalLoad_Analysis.Supports.Count - 1)
            //            {
            //                s2 += Long_Girder_Analysis.TotalLoad_Analysis.Supports[i].NodeNo;
            //            }
            //            else
            //                s2 += Long_Girder_Analysis.TotalLoad_Analysis.Supports[i].NodeNo + ", ";
            //        }
            //    }
            //}
            //catch (Exception ex) { }
            //double BB = MyList.StringToDouble(txt_Ana_B.Text, 8.5);

                _file_Check:



                //grb_create_input_data.Enabled = rbtn_ana_create_analysis_file.Checked;
                grb_select_analysis.Enabled = !rbtn_ana_create_analysis_file.Checked;

                //grb_create_input_data.Enabled = !rbtn_ana_select_analysis_file.Checked;
                grb_select_analysis.Enabled = rbtn_ana_select_analysis_file.Checked;

                Button_Enable_Disable();
                Write_All_Data(false);
                iApp.Progress_Works.Clear();

                #endregion Process
                Write_All_Data(false);
            }
            catch (Exception ex) { }
        }
        private void btn_Deck_Analysis_Click1(object sender, EventArgs e)
        {

            //Write_All_Data(true);
            user_path = IsCreateData ? Path.Combine(iApp.LastDesignWorkingFolder, Title) : user_path;
            if (!Directory.Exists(user_path))
            {
                Directory.CreateDirectory(user_path);
            }
            try
            {
                DECKSLAB_LL_TXT();
                #region Process
                int i = 1;
                Write_All_Data(true);

                string flPath = Deck_Analysis.Input_File;
                do
                {
                    if (i == 1)
                    {
                        flPath = Deck_Analysis.LL_Analysis_1_Input_File;
                    }
                    else if (i == 2)
                    {
                        flPath = Deck_Analysis.LL_Analysis_2_Input_File;
                    }
                    else if (i == 3)
                    {
                        flPath = Deck_Analysis.LL_Analysis_3_Input_File;
                    }
                    else if (i == 4)
                    {

                        flPath = Deck_Analysis.LL_Analysis_4_Input_File;
                    }
                    else if (i == 5)
                    {

                        flPath = Deck_Analysis.LL_Analysis_5_Input_File;
                    }
                    else if (i == 6)
                    {
                        flPath = Deck_Analysis.LL_Analysis_6_Input_File;
                    }
                    else if (i == 7)
                    {
                        flPath = Deck_Analysis.DeadLoadAnalysis_Input_File;
                    }


                    MessageBox.Show(this, "PROCESS ANALYSIS FOR "
                     + Path.GetFileNameWithoutExtension(flPath).ToUpper(), "ASTRA", MessageBoxButtons.OK, MessageBoxIcon.Information);



                    File.WriteAllText(Path.Combine(Path.GetDirectoryName(flPath), "PAT001.tmp"), Path.GetDirectoryName(flPath) + "\\");
                    File.WriteAllText(Path.Combine(iApp.AppFolder, "PAT001.tmp"), Path.GetDirectoryName(flPath) + "\\");
                    //System.Environment.SetEnvironmentVariable("SURVEY", flPath);

                    System.Diagnostics.Process prs = new System.Diagnostics.Process();

                    System.Environment.SetEnvironmentVariable("SURVEY", flPath);
                    System.Environment.SetEnvironmentVariable("ASTRA", flPath);

                    prs.StartInfo.FileName = Path.Combine(Application.StartupPath, "ast001.exe");
                    if (prs.Start())
                        prs.WaitForExit();
                    i++;
                }
                while (i <= 7);
                //while (i < 3) ;

                //string ana_rep_file = Bridge_Analysis.Analysis_Report;
                string ana_rep_file = Deck_Analysis.Get_Analysis_Report_File(Deck_Analysis.LL_Analysis_1_Input_File);

                if (File.Exists(ana_rep_file))
                {

                    iApp.Progress_Works.Clear();
                    //iApp.Progress_Works.Add("Reading Analysis Data from Total Load Analysis Report File (ANALYSIS_REP.TXT)");
                    //iApp.Progress_Works.Add("Set Structure Geometry for Total Load Analysis");
                    //iApp.Progress_Works.Add("Reading Bending Moment & Shear Force from Analysis Result");


                    //iApp.Progress_Works.Add("Reading Analysis Data from Live Load Analysis Report File (ANALYSIS_REP.TXT)");
                    //iApp.Progress_Works.Add("Set Structure Geometry for Live Load Analysis");
                    //iApp.Progress_Works.Add("Reading Bending Moment & Shear Force from Analysis Result");



                    iApp.Progress_Works.Add("Reading Analysis Data from Live Load Analysis 1 Report File (ANALYSIS_REP.TXT)");
                    iApp.Progress_Works.Add("Set Structure Geometry for Live Load Analysis");
                    iApp.Progress_Works.Add("Reading Bending Moment & Shear Force from Analysis Result");



                    iApp.Progress_Works.Add("Reading Analysis Data from Live Load Analysis 2 Report File (ANALYSIS_REP.TXT)");
                    iApp.Progress_Works.Add("Set Structure Geometry for Live Load Analysis");
                    iApp.Progress_Works.Add("Reading Bending Moment & Shear Force from Analysis Result");



                    iApp.Progress_Works.Add("Reading Analysis Data from Live Load Analysis 3 Report File (ANALYSIS_REP.TXT)");
                    iApp.Progress_Works.Add("Set Structure Geometry for Live Load Analysis");
                    iApp.Progress_Works.Add("Reading Bending Moment & Shear Force from Analysis Result");



                    iApp.Progress_Works.Add("Reading Analysis Data from Live Load Analysis 4 Report File (ANALYSIS_REP.TXT)");
                    iApp.Progress_Works.Add("Set Structure Geometry for Live Load Analysis");
                    iApp.Progress_Works.Add("Reading Bending Moment & Shear Force from Analysis Result");



                    iApp.Progress_Works.Add("Reading Analysis Data from Live Load Analysis 5 Report File (ANALYSIS_REP.TXT)");
                    iApp.Progress_Works.Add("Set Structure Geometry for Live Load Analysis");
                    iApp.Progress_Works.Add("Reading Bending Moment & Shear Force from Analysis Result");



                    iApp.Progress_Works.Add("Reading Analysis Data from Live Load Analysis 6 Report File (ANALYSIS_REP.TXT)");
                    iApp.Progress_Works.Add("Set Structure Geometry for Live Load Analysis");
                    iApp.Progress_Works.Add("Reading Bending Moment & Shear Force from Analysis Result");



                    iApp.Progress_Works.Add("Reading Analysis Data from Dead Load Analysis Report File (ANALYSIS_REP.TXT)");

                    iApp.Progress_Works.Add("Reading support reaction forces from Total Load Analysis Report");
                    iApp.Progress_Works.Add("Reading support reaction forces from Live Load Analysis Report");
                    iApp.Progress_Works.Add("Reading support reaction forces from Dead Load Analysis Report");


                    //Deck_Analysis.TotalLoad_Analysis = null;
                    //Long_Girder_Analysis.TotalLoad_Analysis = new BridgeMemberAnalysis(iApp, Long_Girder_Analysis.Total_Analysis_Report);
                    //Long_Girder_Analysis.LiveLoad_Analysis = new BridgeMemberAnalysis(iApp, Long_Girder_Analysis.LiveLoad_Analysis_Report);

                    Deck_Analysis.LiveLoad_1_Analysis = new BridgeMemberAnalysis(iApp,
                        Deck_Analysis.Get_Analysis_Report_File(Deck_Analysis.LL_Analysis_1_Input_File));

                    Deck_Analysis.LiveLoad_2_Analysis = new BridgeMemberAnalysis(iApp,
                        Deck_Analysis.Get_Analysis_Report_File(Deck_Analysis.LL_Analysis_2_Input_File));

                    Deck_Analysis.LiveLoad_3_Analysis = new BridgeMemberAnalysis(iApp,
                        Deck_Analysis.Get_Analysis_Report_File(Deck_Analysis.LL_Analysis_3_Input_File));

                    Deck_Analysis.LiveLoad_4_Analysis = new BridgeMemberAnalysis(iApp,
                        Deck_Analysis.Get_Analysis_Report_File(Deck_Analysis.LL_Analysis_4_Input_File));

                    Deck_Analysis.LiveLoad_5_Analysis = new BridgeMemberAnalysis(iApp,
                        Deck_Analysis.Get_Analysis_Report_File(Deck_Analysis.LL_Analysis_5_Input_File));

                    Deck_Analysis.LiveLoad_6_Analysis = new BridgeMemberAnalysis(iApp,
                        Deck_Analysis.Get_Analysis_Report_File(Deck_Analysis.LL_Analysis_6_Input_File));


                    Deck_Analysis.DeadLoad_Analysis = new BridgeMemberAnalysis(iApp, Deck_Analysis.DeadLoad_Analysis_Report);


                    Show_Deckslab_Moment_Shear();

                    string s1 = "";
                    string s2 = "";
                    try
                    {
                        for (i = 0; i < Minor_Analysis.TotalLoad_Analysis.Supports.Count; i++)
                        {
                            if (i < Minor_Analysis.TotalLoad_Analysis.Supports.Count / 2)
                            {
                                if (i == Minor_Analysis.TotalLoad_Analysis.Supports.Count / 2 - 1)
                                {
                                    s1 += Minor_Analysis.TotalLoad_Analysis.Supports[i].NodeNo;
                                }
                                else
                                    s1 += Minor_Analysis.TotalLoad_Analysis.Supports[i].NodeNo + ",";
                            }
                            else
                            {
                                if (i == Minor_Analysis.TotalLoad_Analysis.Supports.Count - 1)
                                {
                                    s2 += Minor_Analysis.TotalLoad_Analysis.Supports[i].NodeNo;
                                }
                                else
                                    s2 += Minor_Analysis.TotalLoad_Analysis.Supports[i].NodeNo + ", ";
                            }
                        }
                    }
                    catch (Exception ex) { }
                    double BB = MyList.StringToDouble(txt_Ana_B.Text, 8.5);





                }

                //grb_create_input_data.Enabled = rbtn_ana_create_analysis_file.Checked;
                grb_select_analysis.Enabled = !rbtn_ana_create_analysis_file.Checked;

                //grb_create_input_data.Enabled = !rbtn_ana_select_analysis_file.Checked;
                grb_select_analysis.Enabled = rbtn_ana_select_analysis_file.Checked;



                Button_Enable_Disable();
                Write_All_Data(false);
                iApp.Progress_Works.Clear();

                #endregion Process
                Write_All_Data(false);
            }
            catch (Exception ex) { }
        }
        private void btn_Deck_Create_Analysis_Data_Click(object sender, EventArgs e)
        {

            Write_All_Data(true);
            //user_path = IsCreateData ? Path.Combine(iApp.LastDesignWorkingFolder, Title) : user_path;
            if (!Directory.Exists(user_path))
            {
                Directory.CreateDirectory(user_path);
            }
            try
            {
                DECKSLAB_LL_TXT();

                #region Create Data

                Ana_Initialize_Analysis_InputData();

                string usp = Path.Combine(user_path, "Deck Slab Analysis");
                if (!Directory.Exists(usp))
                {
                    Directory.CreateDirectory(usp);
                }
                Deck_Analysis.Input_File = Path.Combine(usp, "INPUT_DATA.TXT");  
                Deck_Analysis.CreateData();
                Deck_Analysis.WriteData_DeadLoad_Analysis(Deck_Analysis.Input_File);

                Calculate_Load_Computation(Deck_Analysis.Outer_Girders_as_String,
                    Deck_Analysis.Inner_Girders_as_String,
                     Deck_Analysis.joints_list_for_load);

                //Deck_Analysis.WriteData_LiveLoad_Analysis(Deck_Analysis.LiveLoadAnalysis_Input_File);

                Deck_Analysis.WriteData_LiveLoad_Analysis(Deck_Analysis.LL_Analysis_1_Input_File, deck_ll);
                Deck_Analysis.WriteData_LiveLoad_Analysis(Deck_Analysis.LL_Analysis_2_Input_File, deck_ll);
                Deck_Analysis.WriteData_LiveLoad_Analysis(Deck_Analysis.LL_Analysis_3_Input_File, deck_ll);
                Deck_Analysis.WriteData_LiveLoad_Analysis(Deck_Analysis.LL_Analysis_4_Input_File, deck_ll);
                Deck_Analysis.WriteData_LiveLoad_Analysis(Deck_Analysis.LL_Analysis_5_Input_File, deck_ll);
                Deck_Analysis.WriteData_LiveLoad_Analysis(Deck_Analysis.LL_Analysis_6_Input_File, deck_ll);

                Deck_Analysis.WriteData_DeadLoad_Analysis(Deck_Analysis.DeadLoadAnalysis_Input_File);

                //Ana_Write_DeckSlab_Load_Data(Deck_Analysis.Input_File, true, false);
                //Ana_Write_DeckSlab_Load_Data(Deck_Analysis.TotalAnalysis_Input_File, true, false);
                //Ana_Write_DeckSlab_Load_Data(Deck_Analysis.LiveLoadAnalysis_Input_File, true, false);

                //Chiranjit [2013 09 24] Without Dead Load
                Ana_Write_DeckSlab_Load_Data(Deck_Analysis.LL_Analysis_1_Input_File, true, false, 1);
                Ana_Write_DeckSlab_Load_Data(Deck_Analysis.LL_Analysis_2_Input_File, true, false, 2);
                Ana_Write_DeckSlab_Load_Data(Deck_Analysis.LL_Analysis_3_Input_File, true, false, 3);
                Ana_Write_DeckSlab_Load_Data(Deck_Analysis.LL_Analysis_4_Input_File, true, false, 4);
                Ana_Write_DeckSlab_Load_Data(Deck_Analysis.LL_Analysis_5_Input_File, true, false, 5);
                Ana_Write_DeckSlab_Load_Data(Deck_Analysis.LL_Analysis_6_Input_File, true, false, 6);

                Ana_Write_DeckSlab_Load_Data(Deck_Analysis.DeadLoadAnalysis_Input_File, false, true, 1);

                //Deck_Analysis.TotalLoad_Analysis = new BridgeMemberAnalysis(iApp, Deck_Analysis.TotalAnalysis_Input_File);

                string ll_txt = Deck_Analysis.LiveLoad_File;

                Deck_Analysis.Live_Load_List = LoadData.GetLiveLoads(ll_txt);

                if (Deck_Analysis.Live_Load_List == null) return;

                Button_Enable_Disable();

               MessageBox.Show(this, "Analysis Input data is created as \"" + Project_Name + "\\INPUT_DATA.TXT\" inside the working folder.",
                  "ASTRA", MessageBoxButtons.OK, MessageBoxIcon.Information);

                #endregion Create Data

                Write_All_Data(false);
               

            }
            catch (Exception ex) { }
        }

        private void btn_restore_ll_data_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (MessageBox.Show("All values will be changed to original default values, want to change ?",
                "ASTRA", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                //if (btn.Name == btn_deck_restore_ll.Name)
                //    Default_Moving_LoadData( dgv_deck_liveloads);
                 if (btn.Name == btn_long_restore_ll.Name)
                    Default_Moving_LoadData(dgv_long_liveloads);
            }
        }

        private void btn_deck_open_input_file_Click(object sender, EventArgs e)
        {
           
        }

        List<string> deck_ll = new List<string>();
        List<string> deck_ll_types = new List<string>();


        List<string> deck_ll_1 = new List<string>();
        List<string> deck_ll_2 = new List<string>();
        List<string> deck_ll_3 = new List<string>();
        List<string> deck_ll_4 = new List<string>();
        List<string> deck_ll_5 = new List<string>();
        List<string> deck_ll_6 = new List<string>();

        public void DECKSLAB_LL_TXT()
        {

        }

        List<string> long_ll = new List<string>();
        List<string> long_ll_types = new List<string>();
        List<List<string>> all_loads = new List<List<string>>();


        #region Chiranjit [2016 07 11]
        List<string> ll_comb = new List<string>();

        public void Store_LL_Combinations(DataGridView dgv_live_loads, DataGridView dgv_loads)
        {
            ll_comb.Clear();
            List<string> com = new List<string>();
            Hashtable ht_cmb = new Hashtable();

            string kStr = "";
            string txt = "";
            int  i = 0;
            for (i = 0; i < dgv_live_loads.RowCount; i++)
            {
                kStr = dgv_live_loads[0, i].Value.ToString();
                txt = dgv_live_loads[1, i].Value.ToString();

                if(kStr.StartsWith("TYPE"))
                {
                    try
                    {
                        ht_cmb.Add(kStr, txt);
                    }
                    catch (Exception exx) { }
                }
            }

            dgv_live_loads.Tag = ht_cmb;


            com.Clear();


            for (i = 0; i < dgv_loads.RowCount; i++)
            {

                kStr = dgv_loads[0, i].Value.ToString();
            
            
                if(kStr.StartsWith("LOAD"))
                {
                    com.Clear();

                    for (int c = 1; c < dgv_loads.ColumnCount; c++)
                    {
                        txt = dgv_loads[c, i].Value.ToString();

                        if (txt == "") break;
                        kStr = ht_cmb[txt] as string;
                        com.Add(kStr);
                    }

                    //List<string> lst_lane1 = new List<string>();
                    //List<string> lst_lane2 = new List<string>();


                    //lst_lane1.AddRange(com);


                    //lst_lane2.Add(lst_lane1[0]);


                    //lst_lane1.RemoveAt(0);


                    //for(int k = 0; )




                    if(com.Count    > 0)
                    {
                        txt = com[0];

                        int lane = 1;

                        for (int j = 1; j < com.Count; j++)
                        {
                            if (txt == com[j]) lane++;
                        }
                        if (lane > 1)
                        {
                            txt = lane + " Lane " + txt;
                        }
                        else
                        {
                            txt = lane + " Lane " + txt;

                            for (int j = 1; j < com.Count; j++)
                            {
                                txt += " + 1 Lane " + com[j];
                            }
                        }
                        ll_comb.Add(txt);
                    }

                }
            }
            dgv_loads.Tag = ll_comb;
        }

        #endregion Chiranjit [2016 07 11]


        public void LONG_GIRDER_LL_TXT()
        {


            Store_LL_Combinations(dgv_long_liveloads, dgv_long_loads);

            int i = 0;
            int c = 0;
            string kStr = "";
            string txt = "";
            long_ll.Clear();
            long_ll_types.Clear();
            all_loads.Clear();
            List<string> long_ll_impact = new List<string>();

            bool flag = false;
            for (i = 0; i < dgv_long_liveloads.RowCount; i++)
            {
                txt = "";

                for (c = 0; c < dgv_long_liveloads.ColumnCount; c++)
                {
                    kStr = dgv_long_liveloads[c, i].Value.ToString();


                    //if (kStr != "" && kStr.StartsWith("TYPE"))
                    //{
                    //    long_ll_types.Add(kStr);
                    //}

                    if(flag)
                    {
                        //if (long_ll_impact.Contains(kStr) == false)
                            long_ll_impact.Add(kStr);

                        flag = false;
                        txt = "";
                        kStr = "";
                        continue;
                    }
                    if (kStr.ToUpper().StartsWith("IMPACT"))
                    {
                        flag = true;
                        continue;
                    }
                    else if (kStr != "" && !kStr.StartsWith("AXLE"))
                    {
                        txt += kStr + " ";
                    }
                }

                if (txt != "" && txt.StartsWith("TYPE"))
                {
                    long_ll_types.Add(txt);
                }
                long_ll.Add(txt);
            }
            long_ll.Add(string.Format(""));
            //long_ll.Add(string.Format("TYPE 6 IRC40RWHEEL"));
            //long_ll.Add(string.Format("12.0 12.0 12.0 7.0 7.0 5.0 "));
            //long_ll.Add(string.Format("1.07 4.27 3.05 1.22 3.66 "));
            //long_ll.Add(string.Format("2.740"));
            i = 0;

            List<string> list = new List<string>();

            List<string> def_load = new List<string>();
            List<double> def_x = new List<double>();
            List<double> def_z = new List<double>();


            List<string> files = new List<string>();


            load_list_1 = new List<string>();
            load_list_2 = new List<string>();
            load_list_3 = new List<string>();
            load_list_4 = new List<string>();
            load_list_5 = new List<string>();
            load_list_6 = new List<string>();
            load_total_7 = new List<string>();

            int fl = 0;
            double xinc = MyList.StringToDouble(txt_XINCR.Text, 0.0);
            double imp_fact = 1.179;



            int count = 0;
            for (i = 0; i < dgv_long_loads.RowCount; i++)
            {
                txt = "";
                fl = 0;
                kStr = dgv_long_loads[0, i].Value.ToString();

                if (kStr == "")
                {
                    list = new List<string>();
                    count++;
                    for (int j = 0; j < def_load.Count; j++)
                    {
                        for (int f = 0; f < long_ll_types.Count; f++)
                        {
                            if (long_ll_types[f].StartsWith(def_load[j]))
                            {
                                //txt = string.Format("{0} {1:f3}", long_ll_types[f], imp_fact);
                                txt = string.Format("{0} {1:f3}", long_ll_types[f], long_ll_impact[f]);
                                break;
                            }
                        }
                        if (!list.Contains(txt))
                            list.Add(txt);
                    }
                    list.Add("LOAD GENERATION " + txt_LL_load_gen.Text);

                    string fn = "";
                    for (int j = 0; j < def_load.Count; j++)
                    {
                        txt = string.Format("{0} {1:f3} 0 {2:f3} XINC {3}", def_load[j], def_x[j], def_z[j], xinc);
                        list.Add(txt);

                        fn = fn + " " + def_load[j]; 
                    }
                    def_load.Clear();
                    def_x.Clear();
                    def_z.Clear();

                    all_loads.Add(list);
                    if (count == 1)
                    {
                        load_list_1.Clear();
                        load_list_1.AddRange(list.ToArray());
                    }
                    else if (count == 2)
                    {

                        load_list_2.Clear();
                        load_list_2.AddRange(list.ToArray());
                    }
                    else if (count == 3)
                    {
                        load_list_3.Clear();
                        load_list_3.AddRange(list.ToArray());
                    }
                    else if (count == 4)
                    {

                        load_list_4.Clear();
                        load_list_4.AddRange(list.ToArray());
                    }
                    else if (count == 5)
                    {

                        load_list_5.Clear();
                        load_list_5.AddRange(list.ToArray());
                    }
                    else if (count == 6)
                    {

                        load_list_6.Clear();
                        load_list_6.AddRange(list.ToArray());

                    }
                    else if (count == 7)
                    {
                        load_total_7.Clear();
                        load_total_7.AddRange(list.ToArray());
                    }
                }

                if (kStr != "" && (kStr.StartsWith("LOAD") || kStr.StartsWith("TOTAL")))
                {
                    fl = 1; //continue;
                }
                else if (kStr != "" && kStr.StartsWith("X"))
                {
                    fl = 2; //continue;
                }
                else if (kStr != "" && kStr.StartsWith("Z"))
                {
                    fl = 3; //continue;
                }
                else
                    continue;
                for (c = 1; c < dgv_long_loads.ColumnCount; c++)
                {
                    kStr = dgv_long_loads[c, i].Value.ToString();

                    if (kStr == "") continue;
                    if (fl == 1)
                        def_load.Add(kStr);
                    else if (fl == 2)
                        def_x.Add(MyList.StringToDouble(kStr, 0.0));
                    else if (fl == 3)
                        def_z.Add(MyList.StringToDouble(kStr, 0.0));
                }
                //def_load.Add(txt);
            }

            fl = 3;

            //Long_Girder_Analysis.LoadList_1 = 
        }

        #region Design of RCC Pier

        private void cmb_pier_2_k_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmb_pier_2_k.SelectedIndex)
            {
                case 0: txt_pier_2_k.Text = "1.50"; break;
                case 1: txt_pier_2_k.Text = "0.66"; break;
                case 2: txt_pier_2_k.Text = "0.50"; break;
                case 3: txt_pier_2_k.Text = ""; txt_pier_2_k.Focus(); break;
            }
        }

        private void btn_RccPier_Report_Click1(object sender, EventArgs e)
        {
            iApp.RunExe(rcc_pier.rep_file_name);
        }

        private void btn_RccPier_Process_Click(object sender, EventArgs e)
        {
            Write_All_Data();
            double MX1, MY1, W1;

            MX1 = MY1 = W1 = 0.0;

            MX1 = MyList.StringToDouble(txt_RCC_Pier_Mx1.Text, 0.0);
            MY1 = MyList.StringToDouble(txt_RCC_Pier_Mz1.Text, 0.0);
            W1 = MyList.StringToDouble(txt_RCC_Pier_W1_supp_reac.Text, 0.0);

            if (MX1 == 0.0 && MY1 == 0.0 && W1 == 0.0)
            {
                string msg = "Design forces are not found from Bridge Deck Analysis in the current folder\n";
                msg += "Please enter the Design Forces manualy.\n\n";
                msg += "For Example : W1  = 6101.1 kN\n";
                msg += "            : MX1 = 274.8 kN-m\n";
                msg += "            : MZ1 = 603.1 kN-m\n";

                MessageBox.Show(msg, "ASTRA");
            }
            else
            {
                if (rcc_pier == null) rcc_pier = new RccPier(iApp);
                rcc_pier.FilePath = user_path;
                RCC_Pier_Initialize_InputData();
                rcc_pier.Calculate_Program();
                //rcc_pier.Write_User_Input();
                rcc_pier.Write_Drawing_File();
                iApp.Save_Form_Record(this, user_path);
                if (File.Exists(rcc_pier.rep_file_name)) { MessageBox.Show(this, "Report file written in " + rcc_pier.rep_file_name, "ASTRA", MessageBoxButtons.OK, MessageBoxIcon.Information); iApp.View_Result(rcc_pier.rep_file_name); }
                rcc_pier.is_process = true;
            }
            Button_Enable_Disable();
        }
        private void btn_RccPier_Drawing_Click1(object sender, EventArgs e)
        {
            //iapp.SetDrawingFile(user_input_file, "PIER");

            string drwg_path = Path.Combine(Application.StartupPath, "DRAWINGS\\RccPierDrawings");
            //System.Environment.SetEnvironmentVariable("ASTRA_DRAWINGS", drwg_path);
            iApp.RunViewer(Drawing_Folder, "RCC_Pier_Worksheet_Design_1");
            //iapp.RunViewer(drwg_path);
        }
        public void RCC_Pier_Initialize_InputData()
        {
            rcc_pier.L1 = 0.0d;
            rcc_pier.W1 = 0.0d;
            rcc_pier.W2 = 0.0d;
            rcc_pier.W3 = 0.0d;
            rcc_pier.W4 = 0.0d;
            rcc_pier.W5 = 0.0d;
            rcc_pier.total_vehicle_load = 0.0d;
            rcc_pier.D1 = 0.0d;
            rcc_pier.D2 = 0.0d;
            rcc_pier.D3 = 0.0d;

            rcc_pier.RL6 = 0.0d;
            rcc_pier.RL5 = 0.0d;
            rcc_pier.RL4 = 0.0d;
            rcc_pier.RL3 = 0.0d;
            rcc_pier.RL2 = 0.0d;
            rcc_pier.RL1 = 0.0d;
            rcc_pier.H1 = 0.0d;
            rcc_pier.H2 = 0.0d;
            rcc_pier.H3 = 0.0d;
            rcc_pier.H4 = 0.0d;
            rcc_pier.H5 = 0.0d;
            rcc_pier.H6 = 0.0d;
            rcc_pier.H7 = 0.0d;
            rcc_pier.H8 = 0.0d;
            rcc_pier.B1 = 0.0d;
            rcc_pier.B2 = 0.0d;
            rcc_pier.B3 = 0.0d;
            rcc_pier.B4 = 0.0d;
            rcc_pier.B5 = 0.0d;
            rcc_pier.B6 = 0.0d;
            rcc_pier.B7 = 0.0d;
            rcc_pier.B8 = 0.0d;
            rcc_pier.B9 = 0.0d;
            rcc_pier.B10 = 0.0d;
            rcc_pier.B11 = 0.0d;
            rcc_pier.B12 = 0.0d;
            rcc_pier.B13 = 0.0d;
            rcc_pier.B14 = 0.0d;
            rcc_pier.B15 = 1.078d;
            rcc_pier.B16 = 0.0d;
            rcc_pier.NR = 0.0d;
            rcc_pier.NP = 0.0d;
            rcc_pier.gama_c = 0.0d;
            rcc_pier.MX1 = 0.0d;
            rcc_pier.MY1 = 0.0d;
            rcc_pier.sigma_s = 0.0d;

            #region Data Input Form 1 Variables
            rcc_pier.L1 = MyList.StringToDouble(txt_RCC_Pier_L.Text, 0.0);
            rcc_pier.w1 = MyList.StringToDouble(txt_RCC_Pier_CW.Text, 0.0);
            rcc_pier.w2 = MyList.StringToDouble(txt_RCC_Pier__B.Text, 0.0);
            rcc_pier.w3 = MyList.StringToDouble(txt_RCC_Pier_Wp.Text, 0.0);


            rcc_pier.a1 = MyList.StringToDouble(txt_RCC_Pier_Hp.Text, 0.0);
            rcc_pier.NB = MyList.StringToDouble(txt_RCC_Pier_NMG.Text, 0.0);
            rcc_pier.d1 = MyList.StringToDouble(txt_RCC_Pier_DMG.Text, 0.0);
            rcc_pier.d2 = MyList.StringToDouble(txt_RCC_Pier_Ds.Text, 0.0);
            rcc_pier.gama_c = MyList.StringToDouble(txt_RCC_Pier_gama_c.Text, 0.0);
            rcc_pier.B1 = MyList.StringToDouble(txt_RCC_Pier_B1.Text, 0.0);
            rcc_pier.B2 = MyList.StringToDouble(txt_RCC_Pier_B2.Text, 0.0);
            rcc_pier.H1 = MyList.StringToDouble(txt_RCC_Pier_H1.Text, 0.0);
            rcc_pier.B3 = MyList.StringToDouble(txt_RCC_Pier_B3.Text, 0.0);
            rcc_pier.B4 = MyList.StringToDouble(txt_RCC_Pier_B4.Text, 0.0);
            rcc_pier.H2 = MyList.StringToDouble(txt_RCC_Pier_H2.Text, 0.0);
            rcc_pier.B5 = MyList.StringToDouble(txt_RCC_Pier_B5.Text, 0.0);
            rcc_pier.B6 = MyList.StringToDouble(txt_RCC_Pier_B6.Text, 0.0);
            rcc_pier.RL1 = MyList.StringToDouble(txt_RCC_Pier_RL1.Text, 0.0);
            rcc_pier.RL2 = MyList.StringToDouble(txt_RCC_Pier_RL2.Text, 0.0);
            rcc_pier.RL3 = MyList.StringToDouble(txt_RCC_Pier_RL3.Text, 0.0);
            rcc_pier.RL4 = MyList.StringToDouble(txt_RCC_Pier_RL4.Text, 0.0);
            rcc_pier.RL5 = MyList.StringToDouble(txt_RCC_Pier_RL5.Text, 0.0);
            rcc_pier.form_lev = MyList.StringToDouble(txt_RCC_Pier_Form_Lev.Text, 0.0);
            rcc_pier.B7 = MyList.StringToDouble(txt_RCC_Pier_B7.Text, 0.0);
            rcc_pier.H3 = MyList.StringToDouble(txt_RCC_Pier_H3.Text, 0.0);
            rcc_pier.H4 = MyList.StringToDouble(txt_RCC_Pier_H4.Text, 0.0);
            rcc_pier.B8 = MyList.StringToDouble(txt_RCC_Pier_B8.Text, 0.0);
            rcc_pier.H5 = MyList.StringToDouble(txt_RCC_Pier_H5.Text, 0.0);
            rcc_pier.H6 = MyList.StringToDouble(txt_RCC_Pier_H6.Text, 0.0);
            rcc_pier.H7 = MyList.StringToDouble(txt_RCC_Pier_H7.Text, 0.0);
            rcc_pier.B9 = MyList.StringToDouble(txt_RCC_Pier_B9.Text, 0.0);
            rcc_pier.B10 = MyList.StringToDouble(txt_RCC_Pier_B10.Text, 0.0);
            rcc_pier.B11 = MyList.StringToDouble(txt_RCC_Pier_B11.Text, 0.0);
            rcc_pier.B12 = MyList.StringToDouble(txt_RCC_Pier_B12.Text, 0.0);
            rcc_pier.B13 = MyList.StringToDouble(txt_RCC_Pier_B13.Text, 0.0);
            rcc_pier.B14 = MyList.StringToDouble(txt_RCC_Pier___B.Text, 0.0);
            rcc_pier.over_all = rcc_pier.H7 + rcc_pier.H5 + rcc_pier.H6;
            //rcc_pier.B15 = MyList.StringToDouble(txt_RCC_Pier_B15.Text, 0.0);


            rcc_pier.p1 = MyList.StringToDouble(txt_RCC_Pier_p1.Text, 0.0);
            rcc_pier.p2 = MyList.StringToDouble(txt_RCC_Pier_p2.Text, 0.0);
            rcc_pier.d_dash = MyList.StringToDouble(txt_RCC_Pier_d_dash.Text, 0.0);

            rcc_pier.D = MyList.StringToDouble(txt_RCC_Pier_D.Text, 0.0);
            rcc_pier.b = MyList.StringToDouble(txt_RCC_Pier_b.Text, 0.0);

            //rcc_pier.Pu = MyList.StringToDouble(txt_Pu.Text, 0.0);
            //rcc_pier.Mux = MyList.StringToDouble(txt_Mux.Text, 0.0);
            //rcc_pier.Muy = MyList.StringToDouble(txt_Muy.Text, 0.0);
            rcc_pier.NP = MyList.StringToDouble(txt_RCC_Pier_NP.Text, 0.0);
            rcc_pier.NR = MyList.StringToDouble(txt_RCC_Pier_NR.Text, 0.0);
            rcc_pier.MX1 = MyList.StringToDouble(txt_RCC_Pier_Mx1.Text, 0.0);
            rcc_pier.MY1 = MyList.StringToDouble(txt_RCC_Pier_Mz1.Text, 0.0);
            rcc_pier.total_vehicle_load = MyList.StringToDouble(txt_RCC_Pier_vehi_load.Text, 0.0);
            rcc_pier.W1 = MyList.StringToDouble(txt_RCC_Pier_W1_supp_reac.Text, 0.0);

            rcc_pier.sigma_s = MyList.StringToDouble(txt_rcc_pier_sigma_st.Text, 0.0);
            rcc_pier.m = MyList.StringToDouble(txt_rcc_pier_m.Text, 0.0);

            //rcc_pier.sigma_s = MyList.StringToDouble(txt_sigma_st.Text, 0.0);
            //rcc_pier.m = MyList.StringToDouble(txt_rcc_pier_m.Text, 0.0);



            rcc_pier.fck1 = MyList.StringToDouble(cmb_rcc_pier_fck.Text, 0.0);
            rcc_pier.perm_flex_stress = MyList.StringToDouble(txt_rcc_pier_sigma_c.Text, 0.0);
            rcc_pier.fy1 = MyList.StringToDouble(cmb_rcc_pier_fy.Text, 0.0);
            rcc_pier.fck2 = MyList.StringToDouble(cmb_rcc_pier_fck.Text, 0.0);
            rcc_pier.fy2 = MyList.StringToDouble(cmb_rcc_pier_fy.Text, 0.0);
            //Chiranjit [2012 06 16]
            rcc_pier.NB = rcc_pier.NP;
            #endregion Data Input Form 1 Variables

            #region Data Input Form 2 Variables
            rcc_pier.P2 = MyList.StringToDouble(txt_pier_2_P2.Text, 0.0);
            rcc_pier.P3 = MyList.StringToDouble(txt_pier_2_P3.Text, 0.0);

            rcc_pier.B16 = MyList.StringToDouble(txt_pier_2_B16.Text, 0.0);
            //rcc_pier.total_pairs = MyList.StringToDouble(txt_pier_2_total_pairs.Text, 0.0);
            rcc_pier.PL = MyList.StringToDouble(txt_pier_2_PL.Text, 0.0);
            rcc_pier.PML = MyList.StringToDouble(txt_pier_2_PML.Text, 0.0);
            rcc_pier.APD = txt_pier_2_APD.Text;
            rcc_pier.PD = txt_pier_2_PD.Text;
            rcc_pier.SC = MyList.StringToDouble(txt_pier_2_SC.Text, 0.0);
            rcc_pier.HHF = MyList.StringToDouble(txt_pier_2_HHF.Text, 0.0);
            rcc_pier.V = MyList.StringToDouble(txt_pier_2_V.Text, 0.0);
            rcc_pier.K = MyList.StringToDouble(txt_pier_2_k.Text, 0.0);
            rcc_pier.CF = MyList.StringToDouble(txt_pier_2_CF.Text, 0.0);
            rcc_pier.LL = MyList.StringToDouble(txt_pier_2_LL.Text, 0.0);
            rcc_pier.Vr = MyList.StringToDouble(txt_pier_2_Vr.Text, 0.0);
            rcc_pier.Itc = MyList.StringToDouble(txt_pier_2_Itc.Text, 0.0);
            rcc_pier.sdia = MyList.StringToDouble(txt_pier_2_sdia.Text, 0.0);
            rcc_pier.sleg = MyList.StringToDouble(txt_pier_2_slegs.Text, 0.0);
            rcc_pier.ldia = MyList.StringToDouble(txt_pier_2_ldia.Text, 0.0);
            rcc_pier.SBC = MyList.StringToDouble(txt_pier_2_SBC.Text, 0.0);

            #endregion Data Input Form 2 Variables




            rcc_pier.rdia = MyList.StringToDouble(txt_RCC_Pier_rdia.Text, 0.0);
            rcc_pier.tdia = MyList.StringToDouble(txt_RCC_Pier_tdia.Text, 0.0);
            


            rcc_pier.hdia = MyList.StringToDouble(txt_pier_2_hdia.Text, 0.0);
            rcc_pier.hlegs = MyList.StringToDouble(txt_pier_2_hlegs.Text, 0.0);
            rcc_pier.vdia = MyList.StringToDouble(txt_pier_2_vdia.Text, 0.0);
            rcc_pier.vlegs = MyList.StringToDouble(txt_pier_2_vlegs.Text, 0.0);
            rcc_pier.vspc = MyList.StringToDouble(txt_pier_2_vspc.Text, 0.0);


        }
        #endregion Design of RCC Pier

        #region Abutment
        private void btn_Abutment_Process_Click(object sender, EventArgs e)
        {
            Write_All_Data();
            Abut = new RCC_AbutmentWall(iApp);
            Abut.FilePath = user_path;
            Abutment_Initialize_InputData();
            Abut.Write_Cantilever__User_input();
            Abut.Calculate_Program(Abut.rep_file_name);
            Abut.Write_Cantilever_Drawing_File();
            iApp.Save_Form_Record(this, user_path);
            MessageBox.Show(this, "Report file written in " + Abut.rep_file_name, "ASTRA", MessageBoxButtons.OK, MessageBoxIcon.Information);
            iApp.View_Result(Abut.rep_file_name);
            Abut.is_process = true;
            Button_Enable_Disable();
        }
        private void btn_Abutment_Report_Click(object sender, EventArgs e)
        {
            iApp.RunExe(Abut.rep_file_name);
        }
        private void Abutment_Initialize_InputData()
        {
            
        }
        #endregion Abutment

        private void btn_dwg_open_Click(object sender, EventArgs e)
        {
            Button b = sender as Button;

            //string draw = Path.Combine(Drawing_Folder, "Drawings of RCC T Girder Bridge");
            string draw = Drawing_Folder;


            string copy_path = Path.Combine(Worksheet_Folder, Path.GetFileName(Excel_Long_Girder));

            //iApp.Form_Drawing_Editor(eBaseDrawings.RCC_T_Girder_LS_LONG_GIRDER, draw, copy_path).ShowDialog();

            //iApp.Form_Drawing_Editor(eBaseDrawings.RCC_T_Girder_LS_CROSS_GIRDER, draw, copy_path).ShowDialog();
            //iApp.Form_Drawing_Editor(eBaseDrawings.RCC_T_Girder_LS_COUNTERFORT_ABUTMENT, draw, copy_path).ShowDialog();

            eOpenDrawingOption opt = iApp.Open_Drawing_Option();

            if (opt == eOpenDrawingOption.Cancel) return;

            if (opt == eOpenDrawingOption.Design_Drawings)
            {
                #region Design

                if (b.Name == btn_dwg_open_Deckslab.Name)
                {
                    iApp.Form_Drawing_Editor(eBaseDrawings.RCC_MINOR_BRIDGE_DECKSLAB, draw, copy_path).ShowDialog();
                }
                else if (b.Name == btn_dwg_open_Counterfort.Name)
                {
                    iApp.Form_Drawing_Editor(eBaseDrawings.RCC_MINOR_BRIDGE_COUNTERFORT_ABUTMENT, draw, copy_path).ShowDialog();
                }
                else if (b.Name == btn_dwg_open_Cantilever.Name)
                {
                    iApp.Form_Drawing_Editor(eBaseDrawings.RCC_MINOR_BRIDGE_CANTILEVER_ABUTMENT, draw, copy_path).ShowDialog();
                }
                else if (b.Name == btn_dwg_open_Pier.Name)
                {
                    iApp.Form_Drawing_Editor(eBaseDrawings.RCC_MINOR_BRIDGE_PIER, draw, copy_path).ShowDialog();
                }
                #endregion Design

            }
            else if (opt == eOpenDrawingOption.Sample_Drawings)
            {


                #region Sample_Drawings

                if (b.Name == btn_dwg_open_Deckslab.Name)
                {
                    iApp.Form_Drawing_Editor(eBaseDrawings.RCC_MINOR_BRIDGE_DECKSLAB, draw, copy_path).ShowDialog();
                }
                else if (b.Name == btn_dwg_open_Counterfort.Name)
                {
                    iApp.RunViewer(Path.Combine(Drawing_Folder, "Counterfort Abutment Drawings"), "BOX_ABUTMENT");
                }
                else if (b.Name == btn_dwg_open_Cantilever.Name)
                {
                    iApp.RunViewer(Path.Combine(Drawing_Folder, "Cantilever Abutment Drawings"), "TBeam_Abutment");
                }
                else if (b.Name == btn_dwg_open_Pier.Name)
                {
                    iApp.RunViewer(Path.Combine(Drawing_Folder, "RCC Pier Drawings"), "TBeam_Pier");
                }
                #endregion Sample_Drawings

            }


        }

        private void cmb_deck_input_files_SelectedIndexChanged(object sender, EventArgs e)
        {
            Deck_Buttons();
        }

        private void Deck_Buttons()
        {
            //ComboBox cmb = cmb_deck_input_files;

            //string filename = "";
            //if (Deck_Analysis != null)
            //{
            //    if (cmb.SelectedIndex == 0) filename = Deck_Analysis.DeadLoadAnalysis_Input_File;
            //    else if (cmb.SelectedIndex == 1) filename = Deck_Analysis.LL_Analysis_1_Input_File;
            //    else if (cmb.SelectedIndex == 2) filename = Deck_Analysis.LL_Analysis_2_Input_File;
            //    else if (cmb.SelectedIndex == 3) filename = Deck_Analysis.LL_Analysis_3_Input_File;
            //    else if (cmb.SelectedIndex == 4) filename = Deck_Analysis.LL_Analysis_4_Input_File;
            //    else if (cmb.SelectedIndex == 5) filename = Deck_Analysis.LL_Analysis_5_Input_File;
            //    else if (cmb.SelectedIndex == 6) filename = Deck_Analysis.LL_Analysis_6_Input_File;
            //    else if (cmb.SelectedIndex == 7) filename = File_DeckSlab_Results;
            //}
            //btn_deck_view_data.Enabled = File.Exists(filename);
            //btn_deck_view_moving.Enabled = File.Exists(MyList.Get_LL_TXT_File(filename)) && File.Exists(MyList.Get_Analysis_Report_File(filename));
            //btn_deck_view_struc.Enabled = File.Exists(filename) && cmb.SelectedIndex != 7;
            //btn_deck_view_report.Enabled = File.Exists(MyList.Get_Analysis_Report_File(filename));
        }

        private void cmb_long_open_file_SelectedIndexChanged(object sender, EventArgs e)
        {
            #region Set File Name
            
            string file_name = "";
            if (Minor_Analysis != null)
            {
                file_name = Get_LongGirder_File(cmb_long_open_file.SelectedIndex);
            }
            #endregion Set File Name

            btn_view_data.Enabled = File.Exists(file_name);
            btn_View_Moving_Load.Enabled = File.Exists(MyList.Get_LL_TXT_File(file_name)) && File.Exists(MyList.Get_Analysis_Report_File(file_name));
            btn_view_structure.Enabled = File.Exists(file_name) && cmb_long_open_file.SelectedIndex != 9;
            btn_view_report.Enabled = File.Exists(MyList.Get_Analysis_Report_File(file_name));
        }

        private void txt_sec_in_mid_lg_w_TextChanged(object sender, EventArgs e)
        {
            Calculate_Section_Properties();
            //leff
        }

        private void Calculate_Section_Properties()
        {

            #region lg_inner_mid

            LG_INNER_MID.W = MyList.StringToDouble(txt_sec_in_mid_lg_w.Text, 0.0);
            LG_INNER_MID.Ds = MyList.StringToDouble(txt_sec_in_mid_lg_Ds.Text, 0.0);
            LG_INNER_MID.wtf = MyList.StringToDouble(txt_sec_in_mid_lg_wtf.Text, 0.0);
            LG_INNER_MID.bwf = MyList.StringToDouble(txt_sec_in_mid_lg_bwf.Text, 0.0);
            LG_INNER_MID.bw = MyList.StringToDouble(txt_sec_in_mid_lg_bw.Text, 0.0);
            LG_INNER_MID.D1 = MyList.StringToDouble(txt_sec_in_mid_lg_D1.Text, 0.0);
            LG_INNER_MID.D2 = MyList.StringToDouble(txt_sec_in_mid_lg_D2.Text, 0.0);
            LG_INNER_MID.D3 = MyList.StringToDouble(txt_sec_in_mid_lg_D3.Text, 0.0);
            LG_INNER_MID.D4 = MyList.StringToDouble(txt_sec_in_mid_lg_D4.Text, 0.0);
            LG_INNER_MID.d = MyList.StringToDouble(txt_sec_in_mid_lg_D.Text, 0.0);

            #endregion lg_inner_mid

            //double a = lg_inner_mid.Composite_Section_Iz;

            #region lg_outer_mid

            LG_OUTER_MID.W = MyList.StringToDouble(txt_sec_out_mid_lg_W.Text, 0.0);
            LG_OUTER_MID.Ds = MyList.StringToDouble(txt_sec_out_mid_lg_Ds.Text, 0.0);
            LG_OUTER_MID.wtf = MyList.StringToDouble(txt_sec_out_mid_lg_wtf.Text, 0.0);
            LG_OUTER_MID.bwf = MyList.StringToDouble(txt_sec_out_mid_lg_bwf.Text, 0.0);
            LG_OUTER_MID.bw = MyList.StringToDouble(txt_sec_out_mid_lg_BW.Text, 0.0);
            LG_OUTER_MID.D1 = MyList.StringToDouble(txt_sec_out_mid_lg_D1.Text, 0.0);
            LG_OUTER_MID.D2 = MyList.StringToDouble(txt_sec_out_mid_lg_D2.Text, 0.0);
            LG_OUTER_MID.D3 = MyList.StringToDouble(txt_sec_out_mid_lg_D3.Text, 0.0);
            LG_OUTER_MID.D4 = MyList.StringToDouble(txt_sec_out_mid_lg_D4.Text, 0.0);
            LG_OUTER_MID.d = MyList.StringToDouble(txt_sec_out_mid_lg_D.Text, 0.0);

            #endregion lg_outer_mid




            #region lg_inner_sup

            LG_INNER_SUP.W = MyList.StringToDouble(txt_sec_in_sup_lg_w.Text, 0.0);
            LG_INNER_SUP.Ds = MyList.StringToDouble(txt_sec_in_sup_lg_Ds.Text, 0.0);
            LG_INNER_SUP.wtf = MyList.StringToDouble(txt_sec_in_sup_lg_wtf.Text, 0.0);
            LG_INNER_SUP.bw = MyList.StringToDouble(txt_sec_in_sup_lg_bw.Text, 0.0);
            LG_INNER_SUP.D1 = MyList.StringToDouble(txt_sec_in_sup_lg_D1.Text, 0.0);
            LG_INNER_SUP.D2 = MyList.StringToDouble(txt_sec_in_sup_lg_D2.Text, 0.0);
            LG_INNER_SUP.d = MyList.StringToDouble(txt_sec_in_sup_lg_D.Text, 0.0);



            #endregion lg_inner_sup

            #region lg_outer_sup

            LG_OUTER_SUP.W = MyList.StringToDouble(txt_sec_out_sup_lg_W.Text, 0.0);
            LG_OUTER_SUP.Ds = MyList.StringToDouble(txt_sec_out_sup_lg_Ds.Text, 0.0);
            LG_OUTER_SUP.wtf = MyList.StringToDouble(txt_sec_out_sup_lg_wtf.Text, 0.0);
            LG_OUTER_SUP.bw = MyList.StringToDouble(txt_sec_out_sup_lg_bw.Text, 0.0);
            LG_OUTER_SUP.D1 = MyList.StringToDouble(txt_sec_out_sup_lg_D1.Text, 0.0);
            LG_OUTER_SUP.D2 = MyList.StringToDouble(txt_sec_out_sup_lg_D2.Text, 0.0);
            LG_OUTER_SUP.d = MyList.StringToDouble(txt_sec_out_sup_lg_D.Text, 0.0);
            #endregion lg_outer_sup


            //            lo=		=	9.000
            //beff  =	∑beff i+bw<b		
            //beff i= 0.2b1+0.1Lo<= 0.2Lo			
            //b1=	1.35		
            //beff  =			2.64


            double _bw = MyList.StringToDouble(txt_sec_int_cg_bw.Text, 0.0);
            //double _lo = 3 * SMG
            double _lo = NCG * SMG;
            double _b1 = (SMG - _bw) / 2.0;
            double _b = 0.2 * _b1 + 0.1 * _lo;

            if (_b < (0.2 * _lo))
            {
                _b =  (2 *_b + _bw);
            }
            else
                _b = 0.2 * _lo;

            txt_sec_int_cg_w.Text = _b.ToString("f3");

            txt_Ana_DCG.Text = (LG_INNER_MID.d - LG_INNER_MID.D4).ToString("f3");


            #region cg_inter

            CG_INTER.W = MyList.StringToDouble(txt_sec_int_cg_w.Text, 0.0);
            CG_INTER.Ds = MyList.StringToDouble(txt_sec_int_cg_Ds.Text, 0.0);
            CG_INTER.bw = MyList.StringToDouble(txt_sec_int_cg_bw.Text, 0.0);
            CG_INTER.D1 = MyList.StringToDouble(txt_sec_int_cg_D1.Text, 0.0);
            CG_INTER.d = MyList.StringToDouble(txt_sec_int_cg_d.Text, 0.0);

            #endregion cg_inter

            #region cg_end
            CG_END.W = MyList.StringToDouble(txt_sec_end_cg_w.Text, 0.0);
            CG_END.Ds = MyList.StringToDouble(txt_sec_end_cg_Ds.Text, 0.0);
            CG_END.bw = MyList.StringToDouble(txt_sec_end_cg_bw.Text, 0.0);
            CG_END.D1 = MyList.StringToDouble(txt_sec_end_cg_D1.Text, 0.0);
            CG_END.d = MyList.StringToDouble(txt_sec_end_cg_d.Text, 0.0);
            #endregion cg_end

            #region Show Results
            txt_smp_i_a_mid.Text = LG_INNER_MID.Girder_Section_A.ToString("f4");
            txt_smp_i_ix_mid.Text = LG_INNER_MID.Girder_Section_Ix.ToString("f4");
            txt_smp_i_iz_mid.Text = LG_INNER_MID.Girder_Section_Iz.ToString("f4");
            txt_smp_i_a_sup.Text = LG_INNER_SUP.Girder_Section_A.ToString("f4");
            txt_smp_i_Ix_sup.Text = LG_INNER_SUP.Girder_Section_Ix.ToString("f4");
            txt_smp_i_Iz_sup.Text = LG_INNER_SUP.Girder_Section_Iz.ToString("f4");

            //txt_smp_i_a_mid.Text = lg_outer_mid.Girder_Section_A.ToString("f4");
            //txt_smp_i_a_sup.Text = lg_outer_mid.Girder_Section_A.ToString("f4");


            txt_smp_ii_a_mid.Text = LG_INNER_MID.Composite_Section_A.ToString("f4");
            txt_smp_ii_a_sup.Text = LG_INNER_SUP.Composite_Section_A.ToString("f4");
            txt_smp_ii_ix_mid.Text = LG_INNER_MID.Composite_Section_Ix.ToString("f4");
            txt_smp_ii_ix_sup.Text = LG_INNER_SUP.Composite_Section_Ix.ToString("f4");
            txt_smp_ii_iz_mid.Text = LG_INNER_MID.Composite_Section_Iz.ToString("f4");
            txt_smp_ii_iz_sup.Text = LG_INNER_SUP.Composite_Section_Iz.ToString("f4");



            txt_smp_iii_a_mid.Text = LG_OUTER_MID.Composite_Section_A.ToString("f4");
            txt_smp_iii_a_sup.Text = LG_OUTER_SUP.Composite_Section_A.ToString("f4");
            txt_smp_iii_ix_mid.Text = LG_OUTER_MID.Composite_Section_Ix.ToString("f4");
            txt_smp_iii_ix_sup.Text = LG_OUTER_SUP.Composite_Section_Ix.ToString("f4");
            txt_smp_iii_iz_mid.Text = LG_OUTER_MID.Composite_Section_Iz.ToString("f4");
            txt_smp_iii_iz_sup.Text = LG_OUTER_SUP.Composite_Section_Iz.ToString("f4");




            txt_smp_iv_a_cg.Text = CG_END.Composite_Section_A.ToString("f4");
            txt_smp_iv_ix_cg.Text = CG_END.Composite_Section_Ix.ToString("f4");
            txt_smp_iv_iz_cg.Text = CG_END.Composite_Section_Iz.ToString("f4");


            txt_smp_v_a_cg.Text = CG_INTER.Composite_Section_A.ToString("f4");
            txt_smp_v_ix_cg.Text = CG_INTER.Composite_Section_Ix.ToString("f4");
            txt_smp_v_iz_cg.Text = CG_INTER.Composite_Section_Iz.ToString("f4");
            #endregion Show Results
        }

        private void txt_pier_2_APD_TextChanged(object sender, EventArgs e)
        {

            txt_pier_2_APD.TextAlign = HorizontalAlignment.Left;
            txt_pier_2_APD.WordWrap = true;

            double b16 = MyList.StringToDouble(txt_pier_2_B16.Text, 0.0);

            string kStr = txt_pier_2_APD.Text.Replace(",", " ").Trim().TrimEnd().TrimStart();
            kStr = MyList.RemoveAllSpaces(kStr);

            MyList mlist = new MyList(kStr, ' ');

            kStr = "";
            try
            {
                for (int i = 0; i < mlist.Count; i++)
                {
                    if (mlist.GetDouble(i) < b16)
                    {
                        kStr += mlist.StringList[i] + ",";
                    }
                }
                kStr = kStr.Substring(0, kStr.Length - 1);
            }
            catch (Exception ex) { }

            txt_pier_2_PD.Text = kStr;
        }

        private void rbtn_ssprt_pinned_CheckedChanged(object sender, EventArgs e)
        {
            chk_esprt_fixed_FX.Enabled = rbtn_esprt_fixed.Checked;
            chk_esprt_fixed_FY.Enabled = rbtn_esprt_fixed.Checked;
            chk_esprt_fixed_FZ.Enabled = rbtn_esprt_fixed.Checked;
            chk_esprt_fixed_MX.Enabled = rbtn_esprt_fixed.Checked;
            chk_esprt_fixed_MY.Enabled = rbtn_esprt_fixed.Checked;
            chk_esprt_fixed_MZ.Enabled = rbtn_esprt_fixed.Checked;

            chk_ssprt_fixed_FX.Enabled = rbtn_ssprt_fixed.Checked;
            chk_ssprt_fixed_FY.Enabled = rbtn_ssprt_fixed.Checked;
            chk_ssprt_fixed_FZ.Enabled = rbtn_ssprt_fixed.Checked;
            chk_ssprt_fixed_MX.Enabled = rbtn_ssprt_fixed.Checked;
            chk_ssprt_fixed_MY.Enabled = rbtn_ssprt_fixed.Checked;
            chk_ssprt_fixed_MZ.Enabled = rbtn_ssprt_fixed.Checked;
        }

        #region British Standard Loading
        private void txt_deck_width_TextChanged(object sender, EventArgs e)
        {
            British_Interactive();

        }


        private void rbtn_HA_HB_CheckedChanged(object sender, EventArgs e)
        {
            British_Interactive();


            if (rbtn_HA_HB.Checked || rbtn_HB.Checked)
            {
                cmb_long_open_file.Items.Clear();
                cmb_long_open_file.Items.Add(string.Format("DEAD LOAD ANALYSIS"));
                cmb_long_open_file.Items.Add(string.Format("LIVE LOAD ANALYSIS"));
                cmb_long_open_file.Items.Add(string.Format("LIVE LOAD ANALYSIS 1"));
                cmb_long_open_file.Items.Add(string.Format("LIVE LOAD ANALYSIS 2"));
                cmb_long_open_file.Items.Add(string.Format("LIVE LOAD ANALYSIS 3"));
                cmb_long_open_file.Items.Add(string.Format("LIVE LOAD ANALYSIS 4"));
                cmb_long_open_file.Items.Add(string.Format("LIVE LOAD ANALYSIS 5"));
                cmb_long_open_file.Items.Add(string.Format("TOTAL ANALYSIS"));
                cmb_long_open_file.Items.Add(string.Format("LONGITUDINAL GIRDER ANALYSIS RESULTS"));
                //tabCtrl.TabPages.Remove(tab_mov_data_Indian);
            }
            else if (rbtn_HA.Checked)
            {
                cmb_long_open_file.Items.Clear();
                cmb_long_open_file.Items.Add(string.Format("DEAD LOAD ANALYSIS"));
                cmb_long_open_file.Items.Add(string.Format("LIVE LOAD ANALYSIS"));
                cmb_long_open_file.Items.Add(string.Format("TOTAL ANALYSIS"));
                cmb_long_open_file.Items.Add(string.Format("LONGITUDINAL GIRDER ANALYSIS RESULTS"));
                //tabCtrl.TabPages.Remove(tab_mov_data_Indian);
            }
            sp_hb.Visible = !rbtn_HA.Checked;

        }
        public bool IsRead = false;
        public void British_Interactive()
        {
            if (IsRead) return;

            double d, lane_width, impf, lf;

            double incr, lgen;

            //txt_ll_british_incr
            int nos_lane, i;


            d = MyList.StringToDouble(txt_deck_width.Text, 0.0);
            lane_width = MyList.StringToDouble(txt_lane_width.Text, 0.0);
            incr = MyList.StringToDouble(txt_ll_british_incr.Text, 0.0);

            if (incr == 0) incr = 1;
            lgen = ((int)(L / incr)) + 1;

            nos_lane = (int)(d / lane_width);

            txt_no_lanes.Text = nos_lane.ToString();
            txt_no_lanes.Enabled = false;

            txt_ll_british_lgen.Text = lgen.ToString();
            txt_ll_british_lgen.Enabled = false;


            chk_HA_1L.Enabled = (nos_lane >= 1);
            chk_HA_2L.Enabled = (nos_lane >= 2);
            chk_HA_3L.Enabled = (nos_lane >= 3);
            chk_HA_4L.Enabled = (nos_lane >= 4);
            chk_HA_5L.Enabled = (nos_lane >= 5);
            chk_HA_6L.Enabled = (nos_lane >= 6);
            chk_HA_7L.Enabled = (nos_lane >= 7);
            chk_HA_8L.Enabled = (nos_lane >= 8);
            chk_HA_9L.Enabled = (nos_lane >= 9);
            chk_HA_10L.Enabled = (nos_lane >= 10);


            chk_HB_1L.Enabled = (nos_lane >= 1);
            chk_HB_2L.Enabled = (nos_lane >= 2);
            chk_HB_3L.Enabled = (nos_lane >= 3);
            chk_HB_4L.Enabled = (nos_lane >= 4);
            chk_HB_5L.Enabled = (nos_lane >= 5);
            chk_HB_6L.Enabled = (nos_lane >= 6);
            chk_HB_7L.Enabled = (nos_lane >= 7);
            chk_HB_8L.Enabled = (nos_lane >= 8);
            chk_HB_9L.Enabled = (nos_lane >= 9);
            chk_HB_10L.Enabled = (nos_lane >= 10);


            grb_ha.Enabled = (rbtn_HA.Checked || rbtn_HA_HB.Checked);
            grb_hb.Enabled = (rbtn_HB.Checked || rbtn_HA_HB.Checked);

            if (rbtn_HA.Checked)
            {
                chk_HA_1L.Checked = chk_HA_1L.Enabled;
                chk_HA_2L.Checked = chk_HA_2L.Enabled;
                chk_HA_3L.Checked = chk_HA_3L.Enabled;
                chk_HA_4L.Checked = chk_HA_4L.Enabled;
                chk_HA_5L.Checked = chk_HA_5L.Enabled;
                chk_HA_6L.Checked = chk_HA_6L.Enabled;
                chk_HA_7L.Checked = chk_HA_7L.Enabled;
                chk_HA_8L.Checked = chk_HA_8L.Enabled;
                chk_HA_9L.Checked = chk_HA_9L.Enabled;
                chk_HA_10L.Checked = chk_HA_10L.Enabled;
            }

            if (rbtn_HB.Checked)
            {
                chk_HB_1L.Checked = chk_HB_1L.Enabled;
                chk_HB_2L.Checked = chk_HB_2L.Enabled;
                chk_HB_3L.Checked = chk_HB_3L.Enabled;
                chk_HB_4L.Checked = chk_HB_4L.Enabled;
                chk_HB_5L.Checked = chk_HB_5L.Enabled;
                chk_HB_6L.Checked = chk_HB_6L.Enabled;
                chk_HB_7L.Checked = chk_HB_7L.Enabled;
                chk_HB_8L.Checked = chk_HB_8L.Enabled;
                chk_HB_9L.Checked = chk_HB_9L.Enabled;
                chk_HB_10L.Checked = chk_HB_10L.Enabled;
            }

            //if(rbtn_HA_HB.Checked)
            //{

            //    chk_HB_1L.Checked = !chk_HA_1L.Checked;
            //    chk_HB_2L.Checked = !chk_HA_2L.Checked;
            //    chk_HB_3L.Checked = !chk_HA_3L.Checked;
            //    chk_HB_4L.Checked = !chk_HA_4L.Checked;
            //    chk_HB_5L.Checked = !chk_HA_5L.Checked;
            //    chk_HB_6L.Checked = !chk_HA_6L.Checked;
            //    chk_HB_7L.Checked = !chk_HA_7L.Checked;
            //    chk_HB_8L.Checked = !chk_HA_8L.Checked;
            //    chk_HB_9L.Checked = !chk_HA_9L.Checked;
            //    chk_HB_10L.Checked = !chk_HA_10L.Checked;
            //}


        }

        public void Default_British_LoadData(DataGridView dgv_live_load)
        {

            List<string> list = new List<string>();
            List<string> lst_spc = new List<string>();
            dgv_live_load.Rows.Clear();

            string load = cmb_HB.Text;
            int i = 0;
            list.Clear();
            int typ_no = 1;


            double ll = MyList.StringToDouble(load.Replace("HB_", ""), 1.0);


            for (i = 6; i <= 26; i += 5)
            {
                list.Add(string.Format("TYPE {0}, {1}_{2}", typ_no++, load, i));
                list.Add(string.Format("AXLE LOAD IN TONS ,{0:f1}, {0:f1}, {0:f1}, {0:f1}", ll));
                list.Add(string.Format("AXLE SPACING IN METRES, 1.8,{0:f1},1.8", i));
                list.Add(string.Format("AXLE WIDTH IN METRES, 1.0"));
                list.Add(string.Format("IMPACT FACTOR, {0}", txt_LL_impf.Text));
                list.Add(string.Format(""));
            }

            for (i = 0; i < dgv_live_load.ColumnCount; i++)
            {
                lst_spc.Add("");
            }
            for (i = 0; i < list.Count; i++)
            {
                dgv_live_load.Rows.Add(lst_spc.ToArray());
            }

            MyList mlist = null;
            for (i = 0; i < list.Count; i++)
            {
                mlist = new MyList(list[i], ',');

                //for (int j = 0; j < mlist.Count; j++)
                //{
                //    dgv_live_load[j, i].Value = mlist[j];
                //}

                try
                {
                    if (list[i] == "")
                    {
                        dgv_live_load.Rows[i].DefaultCellStyle.BackColor = Color.Cyan;

                    }
                    else
                    {
                        for (int j = 0; j < mlist.Count; j++)
                        {
                            dgv_live_load[j, i].Value = mlist[j];
                        }
                    }
                }
                catch (Exception ex) { }
            }
        }
        public void Default_British_Type_LoadData(DataGridView dgv_live_load)
        {
            List<string> lst_spcs = new List<string>();
            dgv_live_load.Rows.Clear();
            int i = 0;
            for (i = 0; i < dgv_live_load.ColumnCount; i++)
            {
                lst_spcs.Add("");
            }
            List<string> list = new List<string>();


            List<int> lanes = new List<int>();

            if (chk_HB_1L.Checked) lanes.Add(1);
            if (chk_HB_2L.Checked) lanes.Add(2);
            if (chk_HB_3L.Checked) lanes.Add(3);
            if (chk_HB_4L.Checked) lanes.Add(4);
            if (chk_HB_5L.Checked) lanes.Add(5);
            if (chk_HB_6L.Checked) lanes.Add(6);
            if (chk_HB_7L.Checked) lanes.Add(7);
            if (chk_HB_8L.Checked) lanes.Add(8);
            if (chk_HB_9L.Checked) lanes.Add(9);
            if (chk_HB_10L.Checked) lanes.Add(10);


            #region Long Girder
            list.Clear();


            double d, lane_width, impf, lf;
            int nos_lane;


            d = MyList.StringToDouble(txt_deck_width.Text, 0.0);
            lane_width = MyList.StringToDouble(txt_lane_width.Text, 0.0);

            nos_lane = (int)(d / lane_width);



            string load = "LOAD 1";
            string x = "X";
            string z = "Z";

            LiveLoadCollections llc = new LiveLoadCollections();

            //llc.D

            #region Load 1

            for (int ld = 1; ld <= 5; ld++)
            {
                load = "LOAD " + ld;
                x = "X";
                z = "Z";

                for (i = 0; i < lanes.Count; i++)
                {
                    load += ",TYPE " + ld;
                    x += ",-" + (1 + 5 * ld + 1.8 + 1.8).ToString();
                    z += "," + ((lanes[i] - 1) * lane_width + 0.25);

                    load += ",TYPE " + ld;
                    x += ",-" + (1 + 5 * ld + 1.8 + 1.8).ToString();
                    z += "," + ((lanes[i] - 1) * lane_width + 0.25 + 1.0 + 1.0);
                }

                list.Add(load);
                list.Add(x);
                list.Add(z);
                list.Add(string.Format(""));
            }
            #endregion Load 1

            //list.Add(string.Format("LOAD 1,TYPE 1"));
            //list.Add(string.Format("X,0"));
            //list.Add(string.Format("Z,1.5"));
            //list.Add(string.Format(""));
            //list.Add(string.Format("LOAD 2,TYPE 2"));
            //list.Add(string.Format("X,0"));
            //list.Add(string.Format("Z,1.5"));
            //list.Add(string.Format(""));
            //list.Add(string.Format("LOAD 3,TYPE 3"));
            //list.Add(string.Format("X,0"));
            //list.Add(string.Format("Z,5.9"));
            //list.Add(string.Format(""));
            //list.Add(string.Format("LOAD 4,TYPE 4"));
            //list.Add(string.Format("X,0"));
            //list.Add(string.Format("Z,1.5"));
            //list.Add(string.Format(""));
            //list.Add(string.Format("LOAD 5,TYPE 5"));
            //list.Add(string.Format("X,0,0"));
            //list.Add(string.Format("Z,1.5,4.5"));
            //list.Add(string.Format(""));
            #endregion




            dgv_live_load.Columns.Clear();

            for (i = 0; i <= lanes.Count * 2; i++)
            {
                if (i == 0)
                {
                    dgv_live_load.Columns.Add("col_brts" + i, "Load Data");
                    dgv_live_load.Columns[i].Width = 70;
                    dgv_live_load.Columns[i].ReadOnly = true;
                }
                else
                {
                    dgv_live_load.Columns.Add("col_brts" + i, i.ToString());
                    dgv_live_load.Columns[i].Width = 50;
                }
            }


            for (i = 0; i < list.Count; i++)
            {
                dgv_live_load.Rows.Add(lst_spcs.ToArray());
            }

            MyList mlist = null;
            for (i = 0; i < list.Count; i++)
            {
                mlist = new MyList(list[i], ',');
                try
                {
                    if (list[i] == "")
                    {
                        dgv_live_load.Rows[i].DefaultCellStyle.BackColor = Color.Cyan;

                    }
                    else
                    {
                        for (int j = 0; j < mlist.Count; j++)
                        {
                            dgv_live_load[j, i].Value = mlist[j];
                        }
                    }
                }
                catch (Exception ex) { }
            }
        }
        private void cmb_HB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRead) return;

            Default_British_LoadData(dgv_long_british_loads);
            Default_British_Type_LoadData(dgv_british_loads);
        }

        private void chk_HA_1L_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRead) return;
            chk_HB_1L.Checked = (!chk_HA_1L.Checked && chk_HB_1L.Enabled);
            chk_HB_2L.Checked = !chk_HA_2L.Checked && chk_HB_2L.Enabled;
            chk_HB_3L.Checked = !chk_HA_3L.Checked && chk_HB_3L.Enabled;
            chk_HB_4L.Checked = !chk_HA_4L.Checked && chk_HB_4L.Enabled;
            chk_HB_5L.Checked = !chk_HA_5L.Checked && chk_HB_5L.Enabled;
            chk_HB_6L.Checked = !chk_HA_6L.Checked && chk_HB_6L.Enabled;
            chk_HB_7L.Checked = !chk_HA_7L.Checked && chk_HB_7L.Enabled;
            chk_HB_8L.Checked = !chk_HA_8L.Checked && chk_HB_8L.Enabled;
            chk_HB_9L.Checked = !chk_HA_9L.Checked && chk_HB_9L.Enabled;
            chk_HB_10L.Checked = !chk_HA_10L.Checked && chk_HB_10L.Enabled;
            Default_British_Type_LoadData(dgv_british_loads);


            //if (!chk_HA_1L.Enabled) chk_HA_1L.Checked = false;
            //if (!chk_HA_2L.Enabled) chk_HA_2L.Checked = false;
            //if (!chk_HA_3L.Enabled) chk_HA_3L.Checked = false;
            //if (!chk_HA_4L.Enabled) chk_HA_4L.Checked = false;
            //if (!chk_HA_5L.Enabled) chk_HA_5L.Checked = false;
            //if (!chk_HA_6L.Enabled) chk_HA_6L.Checked = false;
            //if (!chk_HA_7L.Enabled) chk_HA_7L.Checked = false;
            //if (!chk_HA_8L.Enabled) chk_HA_8L.Checked = false;
            //if (!chk_HA_9L.Enabled) chk_HA_9L.Checked = false;
            //if (!chk_HA_10L.Enabled) chk_HA_10L.Checked = false;



            //if (!chk_HB_1L.Enabled) chk_HB_1L.Checked = false;
            //if (!chk_HB_2L.Enabled) chk_HB_2L.Checked = false;
            //if (!chk_HB_3L.Enabled) chk_HB_3L.Checked = false;
            //if (!chk_HB_4L.Enabled) chk_HB_4L.Checked = false;
            //if (!chk_HB_5L.Enabled) chk_HB_5L.Checked = false;
            //if (!chk_HB_6L.Enabled) chk_HB_6L.Checked = false;
            //if (!chk_HB_7L.Enabled) chk_HB_7L.Checked = false;
            //if (!chk_HB_8L.Enabled) chk_HB_8L.Checked = false;
            //if (!chk_HB_9L.Enabled) chk_HB_9L.Checked = false;
            //if (!chk_HB_10L.Enabled) chk_HB_10L.Checked = false;

        }

        private void chk_HB_1L_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRead) return;
            chk_HA_1L.Checked = !chk_HB_1L.Checked && chk_HB_1L.Enabled;
            chk_HA_2L.Checked = !chk_HB_2L.Checked && chk_HB_2L.Enabled;
            chk_HA_3L.Checked = !chk_HB_3L.Checked && chk_HB_3L.Enabled;
            chk_HA_4L.Checked = !chk_HB_4L.Checked && chk_HB_4L.Enabled;
            chk_HA_5L.Checked = !chk_HB_5L.Checked && chk_HB_5L.Enabled;
            chk_HA_6L.Checked = !chk_HB_6L.Checked && chk_HB_6L.Enabled;
            chk_HA_7L.Checked = !chk_HB_7L.Checked && chk_HB_7L.Enabled;
            chk_HA_8L.Checked = !chk_HB_8L.Checked && chk_HB_8L.Enabled;
            chk_HA_9L.Checked = !chk_HB_9L.Checked && chk_HB_9L.Enabled;
            chk_HA_10L.Checked = !chk_HB_10L.Checked && chk_HB_10L.Enabled;

            Default_British_Type_LoadData(dgv_british_loads);


            //if (!chk_HA_1L.Enabled) chk_HA_1L.Checked = false;
            //if (!chk_HA_2L.Enabled) chk_HA_2L.Checked = false;
            //if (!chk_HA_3L.Enabled) chk_HA_3L.Checked = false;
            //if (!chk_HA_4L.Enabled) chk_HA_4L.Checked = false;
            //if (!chk_HA_5L.Enabled) chk_HA_5L.Checked = false;
            //if (!chk_HA_6L.Enabled) chk_HA_6L.Checked = false;
            //if (!chk_HA_7L.Enabled) chk_HA_7L.Checked = false;
            //if (!chk_HA_8L.Enabled) chk_HA_8L.Checked = false;
            //if (!chk_HA_9L.Enabled) chk_HA_9L.Checked = false;
            //if (!chk_HA_10L.Enabled) chk_HA_10L.Checked = false;



            //if (!chk_HB_1L.Enabled) chk_HB_1L.Checked = false;
            //if (!chk_HB_2L.Enabled) chk_HB_2L.Checked = false;
            //if (!chk_HB_3L.Enabled) chk_HB_3L.Checked = false;
            //if (!chk_HB_4L.Enabled) chk_HB_4L.Checked = false;
            //if (!chk_HB_5L.Enabled) chk_HB_5L.Checked = false;
            //if (!chk_HB_6L.Enabled) chk_HB_6L.Checked = false;
            //if (!chk_HB_7L.Enabled) chk_HB_7L.Checked = false;
            //if (!chk_HB_8L.Enabled) chk_HB_8L.Checked = false;
            //if (!chk_HB_9L.Enabled) chk_HB_9L.Checked = false;
            //if (!chk_HB_10L.Enabled) chk_HB_10L.Checked = false;
        }

        private void chk_HA_3L_EnabledChanged(object sender, EventArgs e)
        {
            if (IsRead) return;
            CheckBox chk = sender as CheckBox;
            if (!chk.Enabled) chk.Checked = false;
        }
        public void LONG_GIRDER_BRITISH_LL_TXT()
        {

            Store_LL_Combinations(dgv_long_british_loads, dgv_british_loads);

            int i = 0;
            int c = 0;
            string kStr = "";
            string txt = "";
            long_ll.Clear();
            long_ll_types.Clear();
            all_loads.Clear();

            if (rbtn_HA.Checked) return;

            List<string> long_ll_impact = new List<string>();


            bool flag = false;
            for (i = 0; i < dgv_long_british_loads.RowCount; i++)
            {
                txt = "";

                for (c = 0; c < dgv_long_british_loads.ColumnCount; c++)
                {
                    kStr = dgv_long_british_loads[c, i].Value.ToString();


                    //if (kStr != "" && kStr.StartsWith("TYPE"))
                    //{
                    //    long_ll_types.Add(kStr);
                    //}

                    if (flag)
                    {
                        long_ll_impact.Add(kStr);
                        flag = false;
                        txt = "";
                        kStr = "";
                        continue;
                    }
                    if (kStr.ToUpper().StartsWith("IMPACT"))
                    {
                        flag = true;
                        continue;
                    }
                    else if (kStr != "" && !kStr.StartsWith("AXLE"))
                    {
                        txt += kStr + " ";
                    }
                }

                if (txt != "" && txt.StartsWith("TYPE"))
                {
                    long_ll_types.Add(txt);
                }
                long_ll.Add(txt);
            }
            long_ll.Add(string.Format(""));
            //long_ll.Add(string.Format("TYPE 6 40RWHEEL"));
            //long_ll.Add(string.Format("12.0 12.0 12.0 7.0 7.0 5.0 "));
            //long_ll.Add(string.Format("1.07 4.27 3.05 1.22 3.66 "));
            //long_ll.Add(string.Format("2.740"));
            i = 0;

            List<string> list = new List<string>();

            List<string> def_load = new List<string>();
            List<double> def_x = new List<double>();
            List<double> def_z = new List<double>();


            load_list_1 = new List<string>();
            load_list_2 = new List<string>();
            load_list_3 = new List<string>();
            load_list_4 = new List<string>();
            load_list_5 = new List<string>();
            load_list_6 = new List<string>();
            load_total_7 = new List<string>();



            int fl = 0;
            double xinc = MyList.StringToDouble(txt_ll_british_incr.Text, 0.5);
            //double imp_fact = 1.179;
            int count = 0;
            for (i = 0; i < dgv_british_loads.RowCount; i++)
            {
                txt = "";
                fl = 0;
                kStr = dgv_british_loads[0, i].Value.ToString();

                if (kStr == "")
                {
                    list = new List<string>();
                    count++;
                    for (int j = 0; j < def_load.Count; j++)
                    {
                        for (int f = 0; f < long_ll_types.Count; f++)
                        {
                            if (long_ll_types[f].StartsWith(def_load[j]))
                            {
                                txt = string.Format("{0} {1:f3}", long_ll_types[f], long_ll_impact[f]);
                                break;
                            }
                        }
                        if (list.Contains(txt) == false)
                            list.Add(txt);
                    }
                    list.Add("LOAD GENERATION " + txt_ll_british_lgen.Text);
                    for (int j = 0; j < def_load.Count; j++)
                    {
                        txt = string.Format("{0} {1:f3} 0 {2:f3} XINC {3}", def_load[j], def_x[j], def_z[j], xinc);
                        list.Add(txt);
                    }
                    def_load.Clear();
                    def_x.Clear();
                    def_z.Clear();

                    all_loads.Add(list);
                    if (count == 1)
                    {
                        load_list_1.Clear();
                        load_list_1.AddRange(list.ToArray());
                    }
                    else if (count == 2)
                    {

                        load_list_2.Clear();
                        load_list_2.AddRange(list.ToArray());
                    }
                    else if (count == 3)
                    {
                        load_list_3.Clear();
                        load_list_3.AddRange(list.ToArray());
                    }
                    else if (count == 4)
                    {

                        load_list_4.Clear();
                        load_list_4.AddRange(list.ToArray());
                    }
                    else if (count == 5)
                    {

                        load_list_5.Clear();
                        load_list_5.AddRange(list.ToArray());
                    }
                    else if (count == 6)
                    {

                        load_list_6.Clear();
                        load_list_6.AddRange(list.ToArray());

                    }
                    else if (count == 7)
                    {
                        load_total_7.Clear();
                        load_total_7.AddRange(list.ToArray());
                    }
                }

                if (kStr != "" && (kStr.StartsWith("LOAD") || kStr.StartsWith("TOTAL")))
                {
                    fl = 1; //continue;
                }
                else if (kStr != "" && kStr.StartsWith("X"))
                {
                    fl = 2; //continue;
                }
                else if (kStr != "" && kStr.StartsWith("Z"))
                {
                    fl = 3; //continue;
                }
                else
                    continue;
                for (c = 1; c < dgv_british_loads.ColumnCount; c++)
                {
                    kStr = dgv_british_loads[c, i].Value.ToString();

                    if (kStr == "") continue;
                    if (fl == 1)
                        def_load.Add(kStr);
                    else if (fl == 2)
                        def_x.Add(MyList.StringToDouble(kStr, 0.0));
                    else if (fl == 3)
                        def_z.Add(MyList.StringToDouble(kStr, 0.0));
                }
                //def_load.Add(txt);
            }

            fl = 3;

            //Long_Girder_Analysis.LoadList_1 = 
        }

        public List<int> HA_Lanes
        {
            get
            {
                List<int> lanes = new List<int>();

                if (chk_HA_1L.Checked) lanes.Add(1);
                if (chk_HA_2L.Checked) lanes.Add(2);
                if (chk_HA_3L.Checked) lanes.Add(3);
                if (chk_HA_4L.Checked) lanes.Add(4);
                if (chk_HA_5L.Checked) lanes.Add(5);
                if (chk_HA_6L.Checked) lanes.Add(6);
                if (chk_HA_7L.Checked) lanes.Add(7);
                if (chk_HA_8L.Checked) lanes.Add(8);
                if (chk_HA_9L.Checked) lanes.Add(9);
                if (chk_HA_10L.Checked) lanes.Add(10);

                return lanes;
            }
        }
        public List<int> HB_Lanes
        {
            get
            {
                List<int> lanes = new List<int>();

                if (chk_HB_1L.Checked) lanes.Add(1);
                if (chk_HB_2L.Checked) lanes.Add(2);
                if (chk_HB_3L.Checked) lanes.Add(3);
                if (chk_HB_4L.Checked) lanes.Add(4);
                if (chk_HB_5L.Checked) lanes.Add(5);
                if (chk_HB_6L.Checked) lanes.Add(6);
                if (chk_HB_7L.Checked) lanes.Add(7);
                if (chk_HB_8L.Checked) lanes.Add(8);
                if (chk_HB_9L.Checked) lanes.Add(9);
                if (chk_HB_10L.Checked) lanes.Add(10);

                return lanes;
            }
        }

        #endregion British Standard Loading

        private void uC_Deckslab_BS1_OnButtonClick(object sender, EventArgs e)
        {
            iApp.Save_Form_Record(this, user_path);
        }

        private void uC_Deckslab_IS1_OnCreateData(object sender, EventArgs e)
        {

            Write_All_Data(true);

            uC_Deckslab_IS1.iApp = iApp;

            uC_Deckslab_IS1.user_path = user_path;





            Calculate_Load_Computation(Minor_Analysis._Outer_Girder_Mid,
                Minor_Analysis._Inner_Girder_Mid,
                 Minor_Analysis.joints_list_for_load);

            uC_Deckslab_IS1.deck_member_load = deck_member_load;


            uC_Deckslab_IS1.L = L;
            uC_Deckslab_IS1.NMG = NMG;
            uC_Deckslab_IS1.SMG = SMG;
            uC_Deckslab_IS1.os = os;
            uC_Deckslab_IS1.CL = CL;
            uC_Deckslab_IS1.Ds = Ds;
            uC_Deckslab_IS1.B = B;


            uC_Deckslab_IS1.Width_LeftCantilever = MyList.StringToDouble(txt_Ana_CL.Text, 0.0);
            uC_Deckslab_IS1.Width_RightCantilever = MyList.StringToDouble(txt_Ana_CR.Text, 0.0);
            //uC_Deckslab_IS1.Skew_Angle = MyList.StringToDouble(dgv_deck_user_input[1, 5].Value.ToString(), 0.0);
            uC_Deckslab_IS1.Number_Of_Long_Girder = MyList.StringToInt(txt_Ana_NMG.Text, 4);
            uC_Deckslab_IS1.Number_Of_Cross_Girder = MyList.StringToInt(txt_Ana_NCG.Text, 3);
            uC_Deckslab_IS1.WidthBridge = L / (NCG - 1);


        }

        private void uC_Deckslab_BS1_OnCreateData(object sender, EventArgs e)
        {
            uC_Deckslab_BS1.iApp = iApp;
            uC_Deckslab_BS1.user_path = user_path;
        }

        private void btn_edit_load_combs_Click(object sender, EventArgs e)
        {
            LimitStateMethod.LoadCombinations.frm_LoadCombination ff = new LoadCombinations.frm_LoadCombination(iApp, dgv_long_liveloads, dgv_long_loads);
            ff.Owner = this;
            ff.ShowDialog();
        }

        private void tc_limit_design_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Text_Changed();


            try
            {
                Text_Changed();
                //uC_Deckslab_IS1.user_path = user_path;
            }
            catch (Exception exx) { }

        }

        private void cmb_NMG_SelectedIndexChanged(object sender, EventArgs e)
        {
            Text_Changed();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 f = new Form1(iApp);
            f.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            Form1 f = new Form1(iApp);
            f.Show();
        }

        private void btn_IRC_Abutment_Click(object sender, EventArgs e)
        {
        //    LimitStateMethod.Abutment.frm_IRC_Abutment fabt = new Abutment.frm_IRC_Abutment(iApp);
        //    fabt.ShowDialog();
        }

        private void txt_LL_load_gen_TextChanged(object sender, EventArgs e)
        {

            //txt_XINCR.Text = ((L) / (MyList.StringToDouble(txt_LL_load_gen.Text, 0.0))).ToString("f0");
        }

        private void btn_TGirder_new_design_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            if (btn.Name == btn_TGirder_browse.Name)
            {
                //user_path = Path.Combine(iApp.LastDesignWorkingFolder, Title);
                frm_Open_Project frm = new frm_Open_Project(this.Name, Path.Combine(iApp.LastDesignWorkingFolder, Title));
                if (frm.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
                {
                    //user_path = Path.Combine(user_path, Project_Name);
                    user_path = frm.Example_Path;
                    iApp.Read_Form_Record(this, frm.Example_Path);
                    txt_project_name.Text = Path.GetFileName(frm.Example_Path);
                    //pile.Project_Name = txt_TGirder_project.Text;
                    //pile.FilePath = user_path;
                    //btn_pile_process.Enabled = true;

                    //Open_Project();
                    //MessageBox.Show("Data Loaded successfully.", "ASTRA", MessageBoxButtons.OK, MessageBoxIcon.Information);




                    #region Save As
                    if (frm.SaveAs_Path != "")
                    {

                        string src_path = user_path;
                        txt_project_name.Text = Path.GetFileName(frm.SaveAs_Path);
                        Create_Project();
                        string dest_path = user_path;
                        MyList.Folder_Copy(src_path, dest_path);
                    }
                    #endregion Save As

                    Open_Project();


                    //Open_Project();

                    txt_project_name.Text = Path.GetFileName(user_path);

                    Write_All_Data();
                }
            }
            else if (btn.Name == btn_TGirder_new_design.Name)
            {
                //frm_NewProject frm = new frm_NewProject(Path.GetDirectoryName(user_path));
                ////frm.Project_Name = "Singlecell Box Culvert Design Project";
                //if (txt_project_name.Text != "")
                //    frm.Project_Name = txt_project_name.Text;
                //else
                //    frm.Project_Name = "Design of RCC T-Girder Bridge";
                //if (frm.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
                //{
                    //txt_project_name.Text = frm.Project_Name;
                    //btn_TGirder_process.Enabled = true;

                //if()
                    IsCreateData = true;
                    Create_Project();
                //}
            }
            Button_Enable_Disable();
        }
        #region Chiranjit [2016 09 07]

        public void All_Button_Enable(bool flag)
        {
            btn_create_data.Enabled = flag;
            //btn_process_analysis.Enabled = flag;
            btn_create_data.Enabled = flag;
            //btn_process_analysis
        }
        eASTRADesignType Project_Type
        {
            get
            {
                return eASTRADesignType.RCC_Minor_Bridge_LS;
            }
        }
        public void Create_Project()
        {
            user_path = Path.Combine(iApp.LastDesignWorkingFolder, Title);
            if (!Directory.Exists(user_path))
            {
                Directory.CreateDirectory(user_path);
            }
            string fname = Path.Combine(user_path, Project_Name + ".apr");

            int ty = (int)Project_Type;
            File.WriteAllText(fname, ty.ToString());
            user_path = Path.Combine(user_path, Project_Name);

            if (Directory.Exists(user_path))
            {
                switch (MessageBox.Show(Project_Name + " is already exist. Do you want overwrite ?",
                    "ASTRA", MessageBoxButtons.YesNoCancel))
                {
                    case System.Windows.Forms.DialogResult.Cancel:
                        return;
                    case System.Windows.Forms.DialogResult.Yes:
                        //Delete Folders
                        Delete_Folder(user_path);
                        break;
                }
            }
            if (!Directory.Exists(user_path))
            {
                Directory.CreateDirectory(user_path);
            }

            Write_All_Data();


            MessageBox.Show(Project_Name + " is Created.", "ASTRA", MessageBoxButtons.OK);
        }

        public void Set_Project_Name()
        {
            string dir = Path.Combine(iApp.LastDesignWorkingFolder, Title);

            string prj_name = "";
            string prj_dir = "";
            int c = 1;
            if (Directory.Exists(dir))
            {
                while (true)
                {
                    prj_name = "DESIGN JOB #" + c.ToString("00");
                    prj_dir = Path.Combine(dir, prj_name);

                    if (!Directory.Exists(prj_dir)) break;
                    c++;
                }
            }
            else
                prj_name = "DESIGN JOB #" + c.ToString("00");

            txt_project_name.Text = prj_name;

        }

        public void Delete_Folder(string folder)
        {
            try
            {
                if (Directory.Exists(folder))
                {
                    foreach (var item in Directory.GetDirectories(folder))
                    {
                        Delete_Folder(item);
                    }
                    foreach (var item in Directory.GetFiles(folder))
                    {
                        File.Delete(item);
                    }
                    Directory.Delete(folder);
                }
            }
            catch (Exception exx) { }
        }

        #endregion Chiranjit [2016 09 07]

        private void uC_RCC_Abut1_Abut_Counterfort_LS1_dead_load_CheckedChanged(object sender, EventArgs e)
        {
            if(uC_RCC_Abut1.uC_Abut_Counterfort_LS1.rbtn_dead_load.Checked)
            {
                uC_RCC_Abut1.uC_Abut_Counterfort_LS1.Reaction_A = txt_dead_vert_reac_kN.Text;
                uC_RCC_Abut1.uC_Abut_Counterfort_LS1.Reaction_B = txt_dead_vert_reac_kN.Text;
            }
        }


    }

    public class RCC_Minor_LS_Analysis
    {

        IApplication iApp;
        public JointNodeCollection Joints { get; set; }
        JointNode[,] Joints_Array;
        Member[,] Long_Girder_Members_Array;
        Member[,] Cross_Girder_Members_Array;
        public MemberCollection MemColls { get; set; }

        public BridgeMemberAnalysis TotalLoad_Analysis = null;
        public BridgeMemberAnalysis LiveLoad_Analysis = null;

        public BridgeMemberAnalysis LiveLoad_1_Analysis = null;
        public BridgeMemberAnalysis LiveLoad_2_Analysis = null;
        public BridgeMemberAnalysis LiveLoad_3_Analysis = null;
        public BridgeMemberAnalysis LiveLoad_4_Analysis = null;
        public BridgeMemberAnalysis LiveLoad_5_Analysis = null;
        public BridgeMemberAnalysis LiveLoad_6_Analysis = null;

        public List<BridgeMemberAnalysis> All_LL_Analysis = null;





        public BridgeMemberAnalysis DeadLoad_Analysis = null;

        public List<LoadData> LoadList_1 = null;
        public List<LoadData> LoadList_2 = null;
        public List<LoadData> LoadList_3 = null;
        public List<LoadData> LoadList_4 = null;
        public List<LoadData> LoadList_5 = null;
        public List<LoadData> LoadList_6 = null;



        public List<LoadData> Live_Load_List = null;
        TotalDeadLoad SIDL = null;

        int _Columns = 0, _Rows = 0;

        double span_length = 0.0;

        //Chiranjit [2012 12 18]
        public TGirder_Section_Properties Long_Inner_Mid_Section { get; set; }
        public TGirder_Section_Properties Long_Inner_Support_Section { get; set; }
        public TGirder_Section_Properties Long_Outer_Mid_Section { get; set; }
        public TGirder_Section_Properties Long_Outer_Support_Section { get; set; }
        public TGirder_Section_Properties Cross_End_Section { get; set; }
        public TGirder_Section_Properties Cross_Intermediate_Section { get; set; }


        //Chiranjit [2013 06 06] Kolkata

        public string List_Envelop_Inner { get; set; }
        public string List_Envelop_Outer { get; set; }

        public string Start_Support { get; set; }
        public string End_Support { get; set; }

        string input_file, user_path;
        public RCC_Minor_LS_Analysis(IApplication thisApp)
        {
            iApp = thisApp;
            input_file = "";
            Length = WidthBridge = Effective_Depth = Skew_Angle = Width_LeftCantilever = 0.0;
            Input_File = "";

            Joints = new JointNodeCollection();
            MemColls = new MemberCollection();
            List_Envelop_Inner = "";
            List_Envelop_Outer = "";
        }

        #region Properties

        public double Length { get; set; }
        public double Ds { get; set; }



        /// <summary>
        /// width of crash barrier
        /// </summary>
        public double Wc { get; set; }
        public double Wf_left { get; set; }
        public double Wf_right { get; set; }
        /// <summary>
        /// width of footpath
        /// </summary>
        public double Wk_left { get; set; }
        public double Wk_right { get; set; }

        /// <summary>
        /// width of railing
        /// </summary>
        public double Wr { get; set; }
        /// <summary>
        /// Overhang of girder off the bearing [og]
        /// </summary>
        public double og { get; set; }
        /// <summary>
        /// Overhang of slab off the bearing [os]
        /// </summary>
        public double os { get; set; }
        /// <summary>
        /// Expansion Gap [eg]
        /// </summary>
        public double eg { get; set; }
        /// <summary>
        /// Length of varring portion
        /// </summary>
        public double Lvp { get; set; }
        /// <summary>
        /// Length of Solid portion
        /// </summary>
        public double Lsp { get; set; }
        /// <summary>
        /// Effective Length
        /// </summary>
        public double Leff { get; set; }


        public double WidthBridge { get; set; }
        public double Effective_Depth { get; set; }
        public int Total_Rows
        {
            get
            {
                return 11;
            }
        }
        public int Total_Columns
        {
            get
            {
                return 11;
            }
        }
        public double Skew_Angle { get; set; }
        public double Width_LeftCantilever { get; set; }
        public double Width_RightCantilever { get; set; }

        public double Spacing_Long_Girder
        {

            get
            {
                //Chiranjit [2013 05 02]
                //return MyList.StringToDouble(((WidthBridge - (2 * Width_LeftCantilever)) / 6.0).ToString("0.000"), 0.0);

                double val = ((WidthBridge - (Width_LeftCantilever + Width_RightCantilever)) / (NMG - 1));
                return MyList.StringToDouble(val.ToString("0.000"), 0.0);
            }
        }
        public double Spacing_Cross_Girder
        {
            get
            {
                //chiranji [2013 05 03]
                //return MyList.StringToDouble(((Length) / 8.0).ToString("0.000"), 0.0);

                double val = (Length - 2 * Effective_Depth) / (NCG - 1);
                return MyList.StringToDouble(val.ToString("0.000"), 0.0);
            }
        }
        public string LiveLoad_File
        {
            get
            {
                return Path.Combine(Working_Folder, "LL.TXT");
            }
        }
        public string Analysis_Report
        {
            get
            {
                return Path.Combine(Working_Folder, "ANALYSIS_REP.TXT");
            }
        }
        #region Analysis Input File
        public string Input_File
        {
            get
            {
                return input_file;
            }
            set
            {
                input_file = value;
                if (File.Exists(value))
                    user_path = Path.GetDirectoryName(input_file);
            }
        }
        //Chiranjit [2012 05 27]
        public string TotalAnalysis_Input_File
        {
            get
            {
                if (Directory.Exists(Working_Folder))
                {
                    string pd = Path.Combine(Working_Folder, "DL + LL Combine Analysis");
                    if (!Directory.Exists(pd)) Directory.CreateDirectory(pd);
                    return Path.Combine(pd, "DL_LL_Combine_Input_File.txt");
                }
                return "";
            }
        }
        //Chiranjit [2012 05 27]
        public string LiveLoadAnalysis_Input_File
        {
            get
            {
                if (Directory.Exists(Working_Folder))
                {
                    string pd = Path.Combine(Working_Folder, "Live Load Analysis");
                    if (!Directory.Exists(pd)) Directory.CreateDirectory(pd);
                    return Path.Combine(pd, "LiveLoad_Analysis_Input_File.txt");
                }
                return "";
            }
        }

        //Chiranjit [2014 10 22]
        public string Get_LL_Analysis_Input_File(int AnalysisNo)
        {
            if (AnalysisNo <= 0) return "";

            if (Directory.Exists(Working_Folder))
            {
                string pd = Path.Combine(Working_Folder, "LL Analysis Load " + AnalysisNo);
                if (!Directory.Exists(pd)) Directory.CreateDirectory(pd);
                return Path.Combine(pd, "LL_Load_" + AnalysisNo + "_Input_File.txt");
            }
            return "";
        }

        //Chiranjit [2013 09 24]
        public string LL_Analysis_1_Input_File
        {
            get
            {
                if (Directory.Exists(Working_Folder))
                {
                    string pd = Path.Combine(Working_Folder, "LL Analysis Load 1");
                    if (!Directory.Exists(pd)) Directory.CreateDirectory(pd);
                    return Path.Combine(pd, "LL_Load_1_Input_File.txt");
                }
                return "";
            }
        }

        public string LL_Analysis_2_Input_File
        {
            get
            {
                if (Directory.Exists(Working_Folder))
                {
                    string pd = Path.Combine(Working_Folder, "LL Analysis Load 2");
                    if (!Directory.Exists(pd)) Directory.CreateDirectory(pd);
                    return Path.Combine(pd, "LL_Load_2_Input_File.txt");
                }
                return "";
            }
        }

        public string LL_Analysis_3_Input_File
        {
            get
            {
                if (Directory.Exists(Working_Folder))
                {
                    string pd = Path.Combine(Working_Folder, "LL Analysis Load 3");
                    if (!Directory.Exists(pd)) Directory.CreateDirectory(pd);
                    return Path.Combine(pd, "LL_Load_3_Input_File.txt");
                }
                return "";
            }
        }


        public string LL_Analysis_4_Input_File
        {
            get
            {
                if (Directory.Exists(Working_Folder))
                {
                    string pd = Path.Combine(Working_Folder, "LL Analysis Load 4");
                    if (!Directory.Exists(pd)) Directory.CreateDirectory(pd);
                    return Path.Combine(pd, "LL_Load_4_Input_File.txt");
                }
                return "";
            }
        }


        public string LL_Analysis_5_Input_File
        {
            get
            {
                if (Directory.Exists(Working_Folder))
                {
                    string pd = Path.Combine(Working_Folder, "LL Analysis Load 5");
                    if (!Directory.Exists(pd)) Directory.CreateDirectory(pd);
                    return Path.Combine(pd, "LL_Load_5_Input_File.txt");
                }
                return "";
            }
        }
        public string LL_Analysis_6_Input_File
        {
            get
            {
                if (Directory.Exists(Working_Folder))
                {
                    string pd = Path.Combine(Working_Folder, "LL Analysis Load 6");
                    if (!Directory.Exists(pd)) Directory.CreateDirectory(pd);
                    return Path.Combine(pd, "LL_Load_6_Input_File.txt");
                    //return Path.Combine(pd, "LL_Type_6_Input_File.txt");
                }
                return "";
            }
        }


        //Chiranjit [2012 05 27]
        public string DeadLoadAnalysis_Input_File
        {
            get
            {
                if (Directory.Exists(Working_Folder))
                {
                    string pd = Path.Combine(Working_Folder, "Dead Load Analysis");
                    if (!Directory.Exists(pd)) Directory.CreateDirectory(pd);
                    return Path.Combine(pd, "DeadLoad_Analysis_Input_File.txt");
                }
                return "";
            }
        }
        public string Total_Analysis_Report
        {
            get
            {
                if (!File.Exists(TotalAnalysis_Input_File)) return "";
                return Path.Combine(Path.GetDirectoryName(TotalAnalysis_Input_File), "ANALYSIS_REP.TXT");

            }
        }
        public string User_Input_Data
        {
            get
            {
                if (!Directory.Exists(Working_Folder)) return "";
                return Path.Combine(Working_Folder, "ASTRA_DATA_FILE.TXT");

            }
        }
        public string LiveLoad_Analysis_Report
        {
            get
            {
                if (!File.Exists(LiveLoadAnalysis_Input_File)) return "";
                return Path.Combine(Path.GetDirectoryName(LiveLoadAnalysis_Input_File), "ANALYSIS_REP.TXT");
            }
        }
        public string DeadLoad_Analysis_Report
        {
            get
            {
                if (!File.Exists(DeadLoadAnalysis_Input_File)) return "";
                return Path.Combine(Path.GetDirectoryName(DeadLoadAnalysis_Input_File), "ANALYSIS_REP.TXT");
            }
        }


        public string Get_Analysis_Report_File(string input_path)
        {

            if (!File.Exists(input_path)) return "";

            return Path.Combine(Path.GetDirectoryName(input_path), "ANALYSIS_REP.TXT");


        }
        public string Working_Folder
        {
            get
            {
                if (File.Exists(Input_File))
                    return Path.GetDirectoryName(Input_File);
                return "";
            }
        }
        public int NoOfInsideJoints
        {
            get
            {
                return 1;
            }
        }
        #endregion Analysis Input File

        //Chiranjit [2013 05 02]
        //public int Number_Of_Long_Girder { get; set; }
        //public int Number_Of_Cross_Girder { get; set; }
        public int NCG { get; set; }
        public int NMG { get; set; }


        #endregion Properties



        //Chiranjit [2013 05 02]
        string support_left_joints = "";
        string support_right_joints = "";

        //Chiranjit [2013 05 03]
        public List<string> joints_list_for_load = new List<string>();


        //Chiranjit [2011 08 01]
        //Create Bridge Input Data by user's given values.
        //Long Girder Spacing, Cross Girder Spacing, Cantilever Width
        public void CreateData()
        {

            //double x_incr = (Length / (Total_Columns - 1));
            //double z_incr = (WidthBridge / (Total_Rows - 1));

            double x_incr = Spacing_Cross_Girder;
            double z_incr = Spacing_Long_Girder;

            JointNode nd;
            //Joints_Array = new JointNode[Total_Rows, Total_Columns];
            //Long_Girder_Members_Array = new Member[Total_Rows, Total_Columns - 1];
            //Cross_Girder_Members_Array = new Member[Total_Rows - 1, Total_Columns];


            int iCols = 0;
            int iRows = 0;

            if (Joints == null)
                Joints = new JointNodeCollection();
            Joints.Clear();

            double skew_length = Math.Tan((Skew_Angle * (Math.PI / 180.0)));

            //double val1 = 12.1;
            double val1 = WidthBridge;
            double val2 = val1 * skew_length;



            double last_x = 0.0;
            double last_z = 0.0;

            List<double> list_x = new List<double>();
            List<double> list_z = new List<double>();
            Hashtable z_table = new Hashtable();

            //Store Joint Coordinates
            double L_2, L_4, eff_d;
            double x_max, x_min;

            //int _Columns, _Rows;

            //_Columns = Total_Columns;
            //_Rows = Total_Rows;

            last_x = 0.0;

            #region Chiranjit [2011 09 23] Correct Create Data

            //Effective_Depth = Lvp;
            Effective_Depth = Lsp;

            list_x.Clear();
            list_x.Add(0.0);

            list_x.Add(og);

            list_x.Add(Lsp);


            //last_x = (Lsp + Lvp);
            //last_x = MyList.StringToDouble(last_x.ToString("0.000"), 0.0);
            //list_x.Add(last_x);

            last_x = Length / 8.0;
            last_x = MyList.StringToDouble(last_x.ToString("0.000"), 0.0);
            list_x.Add(last_x);


            //last_x = 3 * Length / 16.0;
            //last_x = MyList.StringToDouble(last_x.ToString("0.000"), 0.0);
            //list_x.Add(last_x);

            last_x = Length / 4.0;
            last_x = MyList.StringToDouble(last_x.ToString("0.000"), 0.0);
            list_x.Add(last_x);

            //last_x = 5 * Length / 16.0;
            //last_x = MyList.StringToDouble(last_x.ToString("0.000"), 0.0);
            //list_x.Add(last_x);

            last_x = 3 * Length / 8.0;
            last_x = MyList.StringToDouble(last_x.ToString("0.000"), 0.0);
            list_x.Add(last_x);

            //last_x = 7 * Length / 16.0;
            //last_x = MyList.StringToDouble(last_x.ToString("0.000"), 0.0);
            //list_x.Add(last_x);

            last_x = Length / 2.0;
            last_x = MyList.StringToDouble(last_x.ToString("0.000"), 0.0);
            list_x.Add(last_x);

            int i = 0;
            for (i = list_x.Count - 2; i >= 0; i--)
            {
                last_x = Length - list_x[i];
                list_x.Add(last_x);
            }
            MyList.Array_Format_With(ref list_x, "F3");
            last_x = x_incr + Effective_Depth;

            bool flag = true;
            do
            {
                flag = false;
                for (i = 0; i < list_x.Count; i++)
                {
                    if (last_x.ToString("0.00") == list_x[i].ToString("0.00"))
                    {
                        flag = true; break;
                    }
                }

                if (!flag && last_x > Effective_Depth && last_x < (Length - Effective_Depth))
                    list_x.Add(last_x);
                last_x += x_incr;
                last_x = MyList.StringToDouble(last_x.ToString("0.000"), 0.0);

            }
            while (last_x <= Length);
            list_x.Sort();


            list_z.Clear();
            list_z.Add(0);

            if (Wc != 0.0)
            {
                last_z = Wc;
                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
                list_z.Add(last_z);

                last_z = WidthBridge - Wc;
                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
                list_z.Add(last_z);
            }
            else if (Wf_left != 0.0 && Wf_right != 0.0)
            {
                last_z = Wf_left;
                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
                list_z.Add(last_z);


                last_z = WidthBridge - Wf_right;
                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
                list_z.Add(last_z);
            }
            else if (Wf_left != 0.0 && Wf_right == 0.0)
            {
                last_z = Wf_left;
                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
                list_z.Add(last_z);


                last_z = WidthBridge - Wk_right;
                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
                list_z.Add(last_z);
            }
            else if (Wf_left == 0.0 && Wf_right != 0.0)
            {
                last_z = Wk_left;
                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
                list_z.Add(last_z);


                last_z = WidthBridge - Wf_right;
                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
                list_z.Add(last_z);
            }

            if (Width_LeftCantilever != 0.0)
            {
                last_z = Width_LeftCantilever;
                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
                list_z.Add(last_z);
            }

            if (Width_RightCantilever != 0.0)
            {
                last_z = WidthBridge - Width_RightCantilever;
                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
                list_z.Add(last_z);
            }

            last_z = WidthBridge;
            last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
            list_z.Add(last_z);

            last_z = Width_LeftCantilever + z_incr;
            do
            {
                flag = false;
                for (i = 0; i < list_z.Count; i++)
                {
                    if (last_z.ToString("0.00") == list_z[i].ToString("0.00"))
                    {
                        flag = true; break;
                    }
                }

                if (!flag && last_z > Width_LeftCantilever && last_z < (WidthBridge - Width_RightCantilever - 0.2))
                    list_z.Add(last_z);

                last_z += z_incr;

                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);

            } while (last_z <= WidthBridge);

            list_z.Sort();
            #endregion Chiranjit [2011 09 23] Correct Create Data



            _Columns = list_x.Count;
            _Rows = list_z.Count;

            //int i = 0;

            List<double> list = new List<double>();

            for (iRows = 0; iRows < _Rows; iRows++)
            {
                list = new List<double>();
                for (iCols = 0; iCols < _Columns; iCols++)
                {
                    list.Add(list_x[iCols] + list_z[iRows] * skew_length);
                }
                z_table.Add(list_z[iRows], list);
            }

            Joints_Array = new JointNode[_Rows, _Columns];
            Long_Girder_Members_Array = new Member[_Rows, _Columns - 1];
            Cross_Girder_Members_Array = new Member[_Rows - 1, _Columns];


            for (iRows = 0; iRows < _Rows; iRows++)
            {
                list_x = z_table[list_z[iRows]] as List<double>;
                for (iCols = 0; iCols < _Columns; iCols++)
                {
                    nd = new JointNode();
                    nd.Y = 0;
                    nd.Z = list_z[iRows];

                    //nd.X = list_x[iCols] + (skew_length * list_z[iRows]);
                    nd.X = list_x[iCols];

                    nd.NodeNo = Joints.JointNodes.Count + 1;
                    Joints.Add(nd);

                    Joints_Array[iRows, iCols] = nd;

                    last_x = nd.X;
                }
            }
            int nodeNo = 0;
            Joints.Clear();

            //support_left_joints = Joints_Array[0, 0].NodeNo + " TO " + Joints_Array[0, iCols - 1].NodeNo;
            //support_right_joints = Joints_Array[iRows - 1, 0].NodeNo + " TO " + Joints_Array[iRows - 1, iCols - 1].NodeNo;

            support_left_joints = "";
            support_right_joints = "";

            joints_list_for_load.Clear();
            List<int> list_nodes = new List<int>();





            //string str_joints = "";

            for (iCols = 0; iCols < _Columns; iCols++)
            {
                for (iRows = 0; iRows < _Rows; iRows++)
                {
                    nodeNo++;
                    Joints_Array[iRows, iCols].NodeNo = nodeNo;
                    Joints.Add(Joints_Array[iRows, iCols]);

                    if (iCols == 1 && iRows > 1 && iRows < _Rows - 2)
                        support_left_joints += Joints_Array[iRows, iCols].NodeNo + " ";
                    else if (iCols == _Columns - 2 && iRows > 1 && iRows < _Rows - 2)
                        support_right_joints += Joints_Array[iRows, iCols].NodeNo + " ";
                    else
                    {
                        if (iRows > 0 && iRows < _Rows - 1)
                            list_nodes.Add(Joints_Array[iRows, iCols].NodeNo);
                    }
                }
                if (list_nodes.Count > 0)
                {
                    joints_list_for_load.Add(MyList.Get_Array_Text(list_nodes));
                    list_nodes.Clear();
                }
            }

            Member mem = new Member();

            if (MemColls == null)
                MemColls = new MemberCollection();
            MemColls.Clear();


            #region Chiranjit [2013 05 30]
            for (iCols = 0; iCols < _Columns; iCols++)
            {
                for (iRows = 1; iRows < _Rows; iRows++)
                {
                    mem = new Member();
                    mem.StartNode = Joints_Array[iRows - 1, iCols];
                    mem.EndNode = Joints_Array[iRows, iCols];
                    mem.MemberNo = MemColls.Count + 1;
                    MemColls.Add(mem);
                    Cross_Girder_Members_Array[iRows - 1, iCols] = mem;
                }
            }
            for (iRows = 0; iRows < _Rows; iRows++)
            {
                for (iCols = 1; iCols < _Columns; iCols++)
                {
                    mem = new Member();
                    mem.StartNode = Joints_Array[iRows, iCols - 1];
                    mem.EndNode = Joints_Array[iRows, iCols];
                    mem.MemberNo = MemColls.Count + 1;
                    MemColls.Add(mem);
                    Long_Girder_Members_Array[iRows, iCols - 1] = mem;

                }
            }
            #endregion Chiranjit [2013 05 30]


            #region Chiranjit [2013 06 06]

            if (Width_LeftCantilever > 0)
            {
                List_Envelop_Outer = Long_Girder_Members_Array[1, 0].MemberNo + " TO " + Long_Girder_Members_Array[1, iCols - 2].MemberNo;
                List_Envelop_Inner = Long_Girder_Members_Array[2, 0].MemberNo + " TO " + Long_Girder_Members_Array[2, iCols - 2].MemberNo;
            }
            else
            {
                List_Envelop_Outer = Long_Girder_Members_Array[0, 0].MemberNo + " TO " + Long_Girder_Members_Array[0, iCols - 2].MemberNo;
                List_Envelop_Inner = Long_Girder_Members_Array[1, 0].MemberNo + " TO " + Long_Girder_Members_Array[1, iCols - 2].MemberNo;
            }
            #endregion Chiranjit [2013 06 06]
        }

        public string _DeckSlab { get; set; }
        public string _Inner_Girder_Mid { get; set; }
        public string _Inner_Girder_Support { get; set; }
        public string _Outer_Girder_Mid { get; set; }
        public string _Outer_Girder_Support { get; set; }
        public string _Cross_Girder_Inter { get; set; }
        public string _Cross_Girder_End { get; set; }

        //Chiranjit [2011 07 11]
        //Write Default data as given Astra Examples 8

        //string _DeckSlab = "";
        //string _Inner_Girder_Mid = "";
        //string _Inner_Girder_Support = "";
        //string _Outer_Girder_Mid = "";
        //string _Outer_Girder_Support = "";
        //string _Cross_Girder_Inter = "";
        //string _Cross_Girder_End = "";

        void Set_Inner_Outer_Cross_Girders()
        {

            List<int> Inner_Girder = new List<int>();
            List<int> Outer_Girder = new List<int>();
            List<int> Cross_Girder = new List<int>();

            for (int i = 0; i < MemColls.Count; i++)
            {

                if ((MemColls[i].StartNode.Z.ToString("0.000") != MemColls[i].EndNode.Z.ToString("0.000")))
                {
                    Cross_Girder.Add(MemColls[i].MemberNo);
                }
                else if ((MemColls[i].StartNode.Z.ToString("0.000") == Width_LeftCantilever.ToString("0.000") &&
                    MemColls[i].EndNode.Z.ToString("0.000") == Width_LeftCantilever.ToString("0.000")) ||
                    (MemColls[i].StartNode.Z.ToString("0.000") == (WidthBridge - Width_RightCantilever).ToString("0.000") &&
                    MemColls[i].EndNode.Z.ToString("0.000") == (WidthBridge - Width_RightCantilever).ToString("0.000")))
                {
                    Outer_Girder.Add(MemColls[i].MemberNo);
                }
                else if ((MemColls[i].StartNode.Z == 0.0 &&
                    MemColls[i].EndNode.Z == 0.0) ||
                    (MemColls[i].StartNode.Z == WidthBridge) &&
                    (MemColls[i].EndNode.Z == WidthBridge))
                {
                    Outer_Girder.Add(MemColls[i].MemberNo);
                }
                else
                {
                    Inner_Girder.Add(MemColls[i].MemberNo);
                }
            }
            Inner_Girder.Sort();
            Outer_Girder.Sort();
            Cross_Girder.Sort();


            _Cross_Girder_Inter = MyList.Get_Array_Text(Cross_Girder);
            _Inner_Girder_Mid = MyList.Get_Array_Text(Inner_Girder);
            _Outer_Girder_Mid = MyList.Get_Array_Text(Outer_Girder);

        }

        #region Chiranjit [2014 09 02] For British Standard

        public List<int> HA_Lanes;

        public string HA_Loading_Members;
        public void WriteData_Total_Analysis(string file_name, bool is_british)
        {

            string kStr = "";
            List<string> list = new List<string>();
            int i = 0;

            List<int> DeckSlab = new List<int>();

            List<int> Inner_Girder_Mid = new List<int>();
            List<int> Inner_Girder_Support = new List<int>();

            List<int> Outer_Girder_Mid = new List<int>();
            List<int> Outer_Girder_Support = new List<int>();

            List<int> Cross_Girder_Inter = new List<int>();
            List<int> Cross_Girder_End = new List<int>();


            List<int> HA_Members = new List<int>();

            List<double> HA_Dists = new List<double>();
            HA_Dists = new List<double>();
            if (HA_Lanes != null)
            {
                for (i = 0; i < HA_Lanes.Count; i++)
                {
                    HA_Dists.Add(1.75 + (HA_Lanes[i] - 1) * 3.5);
                }
            }

            list.Add("ASTRA FLOOR PSC I GIRDER BRIDGE DECK ANALYSIS");
            list.Add("UNIT METER MTON");
            list.Add("JOINT COORDINATES");
            for (i = 0; i < Joints.Count; i++)
            {
                list.Add(Joints[i].ToString());
            }
            list.Add("MEMBER INCIDENCES");
            for (i = 0; i < MemColls.Count; i++)
            {
                list.Add(MemColls[i].ToString());
            }

            int index = 2;

            for (int c = 0; c < _Rows; c++)
            {
                for (i = 0; i < _Columns - 1; i++)
                {
                    if (i <= 1 || i >= (_Columns - 3))
                    {
                        if (c == index || c == _Rows - index - 1)
                        {
                            Outer_Girder_Support.Add(Long_Girder_Members_Array[c, i].MemberNo);
                        }
                        else if (c > index && c < _Rows - index - 1)
                        {
                            var item = Long_Girder_Members_Array[c, i];

                            if (HA_Dists.Contains(item.EndNode.Z) && HA_Dists.Contains(item.StartNode.Z))
                                HA_Members.Add(item.MemberNo);
                            else
                                Inner_Girder_Support.Add(item.MemberNo);
                        }
                        else
                            DeckSlab.Add(Long_Girder_Members_Array[c, i].MemberNo);
                    }
                    else
                    {
                        if (c == index || c == _Rows - index - 1)
                        {
                            Outer_Girder_Mid.Add(Long_Girder_Members_Array[c, i].MemberNo);
                        }
                        else if (c > index && c < _Rows - index - 1)
                        {

                            var item = Long_Girder_Members_Array[c, i];

                            if (HA_Dists.Contains(item.EndNode.Z) && HA_Dists.Contains(item.StartNode.Z))
                                HA_Members.Add(item.MemberNo);
                            else
                                Inner_Girder_Mid.Add(item.MemberNo);


                            //Inner_Girder_Mid.Add(Long_Girder_Members_Array[c, i].MemberNo);
                        }
                        else
                            DeckSlab.Add(Long_Girder_Members_Array[c, i].MemberNo);
                    }
                }
            }

            Outer_Girder_Mid.Sort();
            Outer_Girder_Support.Sort();


            Inner_Girder_Mid.Sort();
            Inner_Girder_Support.Sort();
            DeckSlab.Sort();
            index = 2;
            List<int> lst_index = new List<int>();
            for (int n = 1; n <= NCG - 2; n++)
            {
                for (i = 0; i < _Columns; i++)
                {
                    if (Cross_Girder_Members_Array[0, i].StartNode.X.ToString("0.00") == (Spacing_Cross_Girder * n + Effective_Depth).ToString("0.00"))
                    {
                        index = i;
                        lst_index.Add(i);
                    }
                }
            }
            for (int c = 0; c < _Rows - 1; c++)
            {
                for (i = 0; i < _Columns - 1; i++)
                {
                    if (lst_index.Contains(i))
                        Cross_Girder_Inter.Add(Cross_Girder_Members_Array[c, i].MemberNo);
                    else if (i == 1 || i == _Columns - 2)
                    {
                        Cross_Girder_End.Add(Cross_Girder_Members_Array[c, i].MemberNo);
                    }
                    else
                        DeckSlab.Add(Cross_Girder_Members_Array[c, i].MemberNo);
                }
            }

            DeckSlab.Sort();
            Cross_Girder_Inter.Sort();
            Cross_Girder_End.Sort();


            //_Cross_Girder_Inter = MyList.Get_Array_Text(Cross_Girder);
            //_Inner_Girder_Mid = MyList.Get_Array_Text(Inner_Girder);
            //_Outer_Girder_Mid = MyList.Get_Array_Text(Outer_Girder);




            //string _DeckSlab = "";
            //string _Inner_Girder_Mid = "";
            //string _Inner_Girder_Support = "";
            //string _Outer_Girder_Mid = "";
            //string _Outer_Girder_Support = "";
            //string _Cross_Girder_Inter = "";
            //string _Cross_Girder_End = "";




            _DeckSlab = MyList.Get_Array_Text(DeckSlab);
            _Inner_Girder_Mid = MyList.Get_Array_Text(Inner_Girder_Mid);
            _Inner_Girder_Support = MyList.Get_Array_Text(Inner_Girder_Support);
            _Outer_Girder_Mid = MyList.Get_Array_Text(Outer_Girder_Mid);
            _Outer_Girder_Support = MyList.Get_Array_Text(Outer_Girder_Support);
            _Cross_Girder_Inter = MyList.Get_Array_Text(Cross_Girder_Inter);
            _Cross_Girder_End = MyList.Get_Array_Text(Cross_Girder_End);



            HA_Loading_Members = MyList.Get_Array_Text(HA_Members);

            list.Add("SECTION PROPERTIES");
            if (Long_Inner_Mid_Section != null)
            {
                Set_Section_Properties(list);
            }
            else
            {
                list.Add(string.Format("{0} TO {1} PRIS AX 1.146 IX 0.022 IZ 0.187", MemColls[0].MemberNo, MemColls[MemColls.Count - 1].MemberNo));

            }
            list.Add("MATERIAL CONSTANTS");
            list.Add("E 2.85E6 ALL");
            //list.Add("E " + Ecm * 100 + " ALL");
            list.Add("DENSITY CONCRETE ALL");
            list.Add("POISSON CONCRETE ALL");
            list.Add("SUPPORT");

            //list.Add(string.Format("{0}  PINNED", support_left_joints));
            //list.Add(string.Format("{0}  FIXED BUT FX MZ", support_right_joints));


            list.Add(string.Format("{0}  {1}", support_left_joints, Start_Support));
            list.Add(string.Format("{0}  {1}", support_right_joints, End_Support));


            list.Add("LOAD 1 DEAD LOAD + SIDL");
            list.Add("**dEAD lOAD");
            list.Add("MEMBER LOAD");
            list.Add("153 TO 158 173 TO 178 UNI GY -2.7504");
            list.Add("151 160 171 180 UNI GY -2.66888");
            list.Add("152 159 172 179 UNI GY -1.68024");
            list.Add("133 TO 138 193 TO 198 UNI GY -2.916");
            list.Add("131 140 191 200 UNI GY -2.97768");
            list.Add("132 139 192 199 UNI GY -1.89528");
            list.Add("1 TO 10 101 TO 110 UNI GY -0.702");
            list.Add("** SIDL");
            list.Add("MEMBER LOAD");
            list.Add("** WEARING COAT");
            list.Add("131 TO 140 191 TO 200 UNI GY -0.68");
            list.Add("151 TO 160 171 TO 180 UNI GY -0.53");
            list.Add("**CRASH BARRIER");
            list.Add("111 TO 120 211 TO 220 UNI GY -1.0");
            list.Add("**** OUTER GIRDER *********");
            //list.Add("DEFINE MOVING LOAD FILE LL.TXT");
            iApp.LiveLoads.Impact_Factor(ref list, iApp.DesignStandard);
            //list.Add("TYPE 1 CLA 1.179");
            //list.Add("TYPE 2 A70R 1.188");
            //list.Add("TYPE 3 A70RT 1.10");
            //list.Add("TYPE 4 CLAR 1.179");
            //list.Add("TYPE 5 A70RR 1.188");
            //list.Add("TYPE 6 A70RR 1.188");
            //list.Add("TYPE 7 A70RR 1.188");
            //list.Add("TYPE 8 A70RR 1.188");
            //list.Add("TYPE 9 A70RR 1.188");
            //list.Add("TYPE 10 A70RR 1.188");
            //list.Add("TYPE 11 A70RR 1.188");
            //list.Add("TYPE 12 A70RR 1.188");
            //list.Add("TYPE 13 A70RR 1.188");
            //list.Add("**** 3 LANE CLASS A *****");
            list.Add("LOAD GENERATION 191");
            list.Add("TYPE 1 -18.8 0 2.75 XINC 0.2");
            list.Add("TYPE 1 -18.8 0 6.25 XINC 0.2");
            list.Add("TYPE 1 -18.8 0 9.75 XINC 0.2");
            //list.Add("**** 3 LANE CLASS A *****");
            //list.Add("*LOAD GENERATION 160");
            //list.Add("*TYPE 1 -18.8 0 2.125 XINC 0.2");
            //list.Add("*TYPE 1 -18.8 0 5.625 XINC 0.2");
            //list.Add("*TYPE 1 -18.8 0 9.125 XINC 0.2");
            //list.Add("*PLOT DISPLACEMENT FILE");
            list.Add("PRINT SUPPORT REACTIONS");
            list.Add("PRINT MAX FORCE ENVELOPE LIST " + List_Envelop_Outer);
            list.Add("PRINT MAX FORCE ENVELOPE LIST " + List_Envelop_Inner);
            list.Add("PERFORM ANALYSIS");
            list.Add("FINISH");
            list.Add(kStr);
            //File.WriteAllLines(file_name, list.ToArray());
            //iApp.Write_LiveLoad_LL_TXT(Path.GetDirectoryName(file_name), true, iApp.DesignStandard);
            //iApp.Write_LiveLoad_LL_TXT(Path.GetDirectoryName(file_name));
            list.Clear();
        }
        public void CreateData_British()
        {

            //double x_incr = (Length / (Total_Columns - 1));
            //double z_incr = (WidthBridge / (Total_Rows - 1));

            double x_incr = Spacing_Cross_Girder;
            double z_incr = Spacing_Long_Girder;

            JointNode nd;
            //Joints_Array = new JointNode[Total_Rows, Total_Columns];
            //Long_Girder_Members_Array = new Member[Total_Rows, Total_Columns - 1];
            //Cross_Girder_Members_Array = new Member[Total_Rows - 1, Total_Columns];


            int iCols = 0;
            int iRows = 0;

            if (Joints == null)
                Joints = new JointNodeCollection();
            Joints.Clear();

            double skew_length = Math.Tan((Skew_Angle * (Math.PI / 180.0)));

            //double val1 = 12.1;
            double val1 = WidthBridge;
            double val2 = val1 * skew_length;



            double last_x = 0.0;
            double last_z = 0.0;

            List<double> list_x = new List<double>();
            List<double> list_z = new List<double>();
            Hashtable z_table = new Hashtable();

            //Store Joint Coordinates
            double L_2, L_4, eff_d;
            double x_max, x_min;

            //int _Columns, _Rows;

            //_Columns = Total_Columns;
            //_Rows = Total_Rows;

            last_x = 0.0;

            #region Chiranjit [2011 09 23] Correct Create Data

            //Effective_Depth = Lvp;
            Effective_Depth = Lsp;

            list_x.Clear();
            list_x.Add(0.0);

            list_x.Add(og);

            list_x.Add(Lsp);


            //last_x = (Lsp + Lvp);
            //last_x = MyList.StringToDouble(last_x.ToString("0.000"), 0.0);
            //list_x.Add(last_x);

            last_x = Length / 8.0;
            last_x = MyList.StringToDouble(last_x.ToString("0.000"), 0.0);
            list_x.Add(last_x);


            //last_x = 3 * Length / 16.0;
            //last_x = MyList.StringToDouble(last_x.ToString("0.000"), 0.0);
            //list_x.Add(last_x);

            last_x = Length / 4.0;
            last_x = MyList.StringToDouble(last_x.ToString("0.000"), 0.0);
            list_x.Add(last_x);

            //last_x = 5 * Length / 16.0;
            //last_x = MyList.StringToDouble(last_x.ToString("0.000"), 0.0);
            //list_x.Add(last_x);

            last_x = 3 * Length / 8.0;
            last_x = MyList.StringToDouble(last_x.ToString("0.000"), 0.0);
            list_x.Add(last_x);

            //last_x = 7 * Length / 16.0;
            //last_x = MyList.StringToDouble(last_x.ToString("0.000"), 0.0);
            //list_x.Add(last_x);

            last_x = Length / 2.0;
            last_x = MyList.StringToDouble(last_x.ToString("0.000"), 0.0);
            list_x.Add(last_x);

            int i = 0;
            for (i = list_x.Count - 2; i >= 0; i--)
            {
                last_x = Length - list_x[i];
                list_x.Add(last_x);
            }
            MyList.Array_Format_With(ref list_x, "F3");
            last_x = x_incr + Effective_Depth;

            bool flag = true;
            do
            {
                flag = false;
                for (i = 0; i < list_x.Count; i++)
                {
                    if (last_x.ToString("0.00") == list_x[i].ToString("0.00"))
                    {
                        flag = true; break;
                    }
                }

                if (!flag && last_x > Effective_Depth && last_x < (Length - Effective_Depth))
                    list_x.Add(last_x);
                last_x += x_incr;
                last_x = MyList.StringToDouble(last_x.ToString("0.000"), 0.0);

            }
            while (last_x <= Length);
            list_x.Sort();


            list_z.Clear();
            list_z.Add(0);

            if (Wc != 0.0)
            {
                last_z = Wc;
                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
                list_z.Add(last_z);

                last_z = WidthBridge - Wc;
                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
                list_z.Add(last_z);
            }
            else if (Wf_left != 0.0 && Wf_right != 0.0)
            {
                last_z = Wf_left;
                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
                list_z.Add(last_z);


                last_z = WidthBridge - Wf_right;
                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
                list_z.Add(last_z);
            }
            else if (Wf_left != 0.0 && Wf_right == 0.0)
            {
                last_z = Wf_left;
                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
                list_z.Add(last_z);


                last_z = WidthBridge - Wk_right;
                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
                list_z.Add(last_z);
            }
            else if (Wf_left == 0.0 && Wf_right != 0.0)
            {
                last_z = Wk_left;
                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
                list_z.Add(last_z);


                last_z = WidthBridge - Wf_right;
                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
                list_z.Add(last_z);
            }

            if (Width_LeftCantilever != 0.0)
            {
                last_z = Width_LeftCantilever;
                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
                list_z.Add(last_z);
            }

            if (Width_RightCantilever != 0.0)
            {
                last_z = WidthBridge - Width_RightCantilever;
                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
                list_z.Add(last_z);
            }

            last_z = WidthBridge;
            last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);
            list_z.Add(last_z);

            last_z = Width_LeftCantilever + z_incr;
            do
            {
                flag = false;
                for (i = 0; i < list_z.Count; i++)
                {
                    if (last_z.ToString("0.00") == list_z[i].ToString("0.00"))
                    {
                        flag = true; break;
                    }
                }

                if (!flag && last_z > Width_LeftCantilever && last_z < (WidthBridge - Width_RightCantilever - 0.2))
                    list_z.Add(last_z);
                last_z += z_incr;
                last_z = MyList.StringToDouble(last_z.ToString("0.000"), 0.0);

            } while (last_z <= WidthBridge);




            //HA_Lanes.Add(1);
            //HA_Lanes.Add(2);
            //HA_Lanes.Add(3);


            List<double> HA_distances = new List<double>();
            if (HA_Lanes.Count > 0)
            {
                double ha = 0.0;

                for (i = 0; i < HA_Lanes.Count; i++)
                {
                    ha = 1.75 + (HA_Lanes[i] - 1) * 3.5;
                    if (!list_z.Contains(ha))
                    {
                        list_z.Add(ha);
                        HA_distances.Add(ha);
                    }
                }
            }

            list_z.Sort();

            #endregion Chiranjit [2011 09 23] Correct Create Data



            _Columns = list_x.Count;
            _Rows = list_z.Count;

            //int i = 0;

            List<double> list = new List<double>();

            for (iRows = 0; iRows < _Rows; iRows++)
            {
                list = new List<double>();
                for (iCols = 0; iCols < _Columns; iCols++)
                {
                    list.Add(list_x[iCols] + list_z[iRows] * skew_length);
                }
                z_table.Add(list_z[iRows], list);
            }

            Joints_Array = new JointNode[_Rows, _Columns];
            Long_Girder_Members_Array = new Member[_Rows, _Columns - 1];
            Cross_Girder_Members_Array = new Member[_Rows - 1, _Columns];



            for (iRows = 0; iRows < _Rows; iRows++)
            {
                list_x = z_table[list_z[iRows]] as List<double>;
                for (iCols = 0; iCols < _Columns; iCols++)
                {
                    nd = new JointNode();
                    nd.Y = 0;
                    nd.Z = list_z[iRows];

                    //nd.X = list_x[iCols] + (skew_length * list_z[iRows]);
                    nd.X = list_x[iCols];

                    nd.NodeNo = Joints.JointNodes.Count + 1;
                    Joints.Add(nd);

                    Joints_Array[iRows, iCols] = nd;

                    last_x = nd.X;
                }
            }
            int nodeNo = 0;
            Joints.Clear();

            //support_left_joints = Joints_Array[0, 0].NodeNo + " TO " + Joints_Array[0, iCols - 1].NodeNo;
            //support_right_joints = Joints_Array[iRows - 1, 0].NodeNo + " TO " + Joints_Array[iRows - 1, iCols - 1].NodeNo;

            support_left_joints = "";
            support_right_joints = "";

            joints_list_for_load.Clear();
            List<int> list_nodes = new List<int>();

            //string str_joints = "";

            for (iCols = 0; iCols < _Columns; iCols++)
            {
                for (iRows = 0; iRows < _Rows; iRows++)
                {
                    nodeNo++;
                    Joints_Array[iRows, iCols].NodeNo = nodeNo;
                    Joints.Add(Joints_Array[iRows, iCols]);

                    if (iCols == 1 && iRows > 1 && iRows < _Rows - 2)
                        support_left_joints += Joints_Array[iRows, iCols].NodeNo + " ";
                    else if (iCols == _Columns - 2 && iRows > 1 && iRows < _Rows - 2)
                        support_right_joints += Joints_Array[iRows, iCols].NodeNo + " ";
                    else
                    {
                        if (iRows > 0 && iRows < _Rows - 1)
                            list_nodes.Add(Joints_Array[iRows, iCols].NodeNo);
                    }
                }
                if (list_nodes.Count > 0)
                {
                    joints_list_for_load.Add(MyList.Get_Array_Text(list_nodes));
                    list_nodes.Clear();
                }
            }

            Member mem = new Member();

            if (MemColls == null)
                MemColls = new MemberCollection();
            MemColls.Clear();
            #region Chiranjit [2013 05 30]
            for (iCols = 0; iCols < _Columns; iCols++)
            {
                for (iRows = 1; iRows < _Rows; iRows++)
                {
                    mem = new Member();
                    mem.StartNode = Joints_Array[iRows - 1, iCols];
                    mem.EndNode = Joints_Array[iRows, iCols];
                    mem.MemberNo = MemColls.Count + 1;
                    MemColls.Add(mem);
                    Cross_Girder_Members_Array[iRows - 1, iCols] = mem;
                }
            }
            for (iRows = 0; iRows < _Rows; iRows++)
            {
                for (iCols = 1; iCols < _Columns; iCols++)
                {
                    mem = new Member();
                    mem.StartNode = Joints_Array[iRows, iCols - 1];
                    mem.EndNode = Joints_Array[iRows, iCols];
                    mem.MemberNo = MemColls.Count + 1;
                    MemColls.Add(mem);
                    Long_Girder_Members_Array[iRows, iCols - 1] = mem;

                }
            }
            #endregion Chiranjit [2013 05 30]

            #region Chiranjit [2013 06 06]

            if (Width_LeftCantilever > 0)
            {
                List_Envelop_Outer = Long_Girder_Members_Array[1, 0].MemberNo + " TO " + Long_Girder_Members_Array[1, iCols - 2].MemberNo;
                List_Envelop_Inner = Long_Girder_Members_Array[2, 0].MemberNo + " TO " + Long_Girder_Members_Array[2, iCols - 2].MemberNo;
            }
            else
            {
                List_Envelop_Outer = Long_Girder_Members_Array[0, 0].MemberNo + " TO " + Long_Girder_Members_Array[0, iCols - 2].MemberNo;
                List_Envelop_Inner = Long_Girder_Members_Array[1, 0].MemberNo + " TO " + Long_Girder_Members_Array[1, iCols - 2].MemberNo;
            }
            #endregion Chiranjit [2013 06 06]
        }

        #endregion Chiranjit [2014 09 02] For British Standard

        public void WriteData_Total_Analysis(string file_name)
        {
            string kStr = "";
            List<string> list = new List<string>();
            int i = 0;

            List<int> DeckSlab = new List<int>();

            List<int> Inner_Girder_Mid = new List<int>();
            List<int> Inner_Girder_Support = new List<int>();

            List<int> Outer_Girder_Mid = new List<int>();
            List<int> Outer_Girder_Support = new List<int>();

            List<int> Cross_Girder_Inter = new List<int>();
            List<int> Cross_Girder_End = new List<int>();






            list.Add("ASTRA FLOOR RCC T GIRDER BRIDGE DECK ANALYSIS");
            list.Add("UNIT METER MTON");
            list.Add("JOINT COORDINATES");
            for (i = 0; i < Joints.Count; i++)
            {
                list.Add(Joints[i].ToString());
            }
            list.Add("MEMBER INCIDENCES");
            for (i = 0; i < MemColls.Count; i++)
            {
                list.Add(MemColls[i].ToString());

                //DeckSlab.Add(MemColls[i].MemberNo);
            }

            int index = 2;





            for (int c = 0; c < _Rows; c++)
            {
                for (i = 0; i < _Columns - 1; i++)
                {
                    if (i <= 1 || i >= (_Columns - 3))
                    {
                        if (c == index || c == _Rows - index - 1)
                        {
                            Outer_Girder_Support.Add(Long_Girder_Members_Array[c, i].MemberNo);
                        }
                        else if (c > index && c < _Rows - index - 1)
                        {
                            Inner_Girder_Support.Add(Long_Girder_Members_Array[c, i].MemberNo);
                        }
                        else
                            DeckSlab.Add(Long_Girder_Members_Array[c, i].MemberNo);
                    }
                    else
                    {
                        if (c == index || c == _Rows - index - 1)
                        {
                            Outer_Girder_Mid.Add(Long_Girder_Members_Array[c, i].MemberNo);
                        }
                        else if (c > index && c < _Rows - index - 1)
                        {
                            Inner_Girder_Mid.Add(Long_Girder_Members_Array[c, i].MemberNo);
                        }
                        else
                            DeckSlab.Add(Long_Girder_Members_Array[c, i].MemberNo);
                    }
                }
            }

            Outer_Girder_Mid.Sort();
            Outer_Girder_Support.Sort();


            Inner_Girder_Mid.Sort();
            Inner_Girder_Support.Sort();
            DeckSlab.Sort();
            index = 2;
            List<int> lst_index = new List<int>();
            for (int n = 1; n <= NCG - 2; n++)
            {
                for (i = 0; i < _Columns; i++)
                {
                    if (Cross_Girder_Members_Array[0, i].StartNode.X.ToString("0.00") == (Spacing_Cross_Girder * n + Effective_Depth).ToString("0.00"))
                    {
                        index = i;
                        lst_index.Add(i);
                    }
                }
            }
            for (int c = 0; c < _Rows - 1; c++)
            {
                for (i = 0; i < _Columns; i++)
                {
                    if (lst_index.Contains(i))
                        Cross_Girder_Inter.Add(Cross_Girder_Members_Array[c, i].MemberNo);
                    else if (i == 1 || i == _Columns - 2)
                    {
                        Cross_Girder_End.Add(Cross_Girder_Members_Array[c, i].MemberNo);
                    }
                    else
                        DeckSlab.Add(Cross_Girder_Members_Array[c, i].MemberNo);
                }
            }

            DeckSlab.Sort();
            Cross_Girder_Inter.Sort();
            Cross_Girder_End.Sort();


            //_Cross_Girder_Inter = MyList.Get_Array_Text(Cross_Girder);
            //_Inner_Girder_Mid = MyList.Get_Array_Text(Inner_Girder);
            //_Outer_Girder_Mid = MyList.Get_Array_Text(Outer_Girder);




            //string _DeckSlab = "";
            //string _Inner_Girder_Mid = "";
            //string _Inner_Girder_Support = "";
            //string _Outer_Girder_Mid = "";
            //string _Outer_Girder_Support = "";
            //string _Cross_Girder_Inter = "";
            //string _Cross_Girder_End = "";




            _DeckSlab = MyList.Get_Array_Text(DeckSlab);
            _Inner_Girder_Mid = MyList.Get_Array_Text(Inner_Girder_Mid);
            _Inner_Girder_Support = MyList.Get_Array_Text(Inner_Girder_Support);
            _Outer_Girder_Mid = MyList.Get_Array_Text(Outer_Girder_Mid);
            _Outer_Girder_Support = MyList.Get_Array_Text(Outer_Girder_Support);
            _Cross_Girder_Inter = MyList.Get_Array_Text(Cross_Girder_Inter);
            _Cross_Girder_End = MyList.Get_Array_Text(Cross_Girder_End);


            list.Add("SECTION PROPERTIES");

            if (Long_Inner_Mid_Section != null)
            {
                Set_Section_Properties(list);
            }
            else
            {
                list.Add(string.Format("{0} TO {1} PRIS AX 1.146 IX 0.022 IZ 0.187", MemColls[0].MemberNo, MemColls[MemColls.Count - 1].MemberNo));

            }
            list.Add("MATERIAL CONSTANT");
            list.Add("E 2.85E6 ALL");
            list.Add("DENSITY CONCRETE ALL");
            list.Add("POISSON CONCRETE ALL");
            list.Add("SUPPORT");
            //list.Add(string.Format("{0}  PINNED", support_left_joints));
            ////list.Add(string.Format("{0}  PINNED", support_right_joints));
            //list.Add(string.Format("{0}  FIXED BUT FX MZ", support_right_joints));


            list.Add(string.Format("{0}  {1}", support_left_joints, Start_Support));
            //list.Add(string.Format("{0}  PINNED", support_right_joints));
            list.Add(string.Format("{0}  {1}", support_right_joints, End_Support));



            //list.Add("111 112 113 114 115 116 117 118 119 120 121  PINNED");
            //list.Add("1 3 5 7 9 11 PINNED");
            //list.Add("111 113 115 117 119 121 PINNED");
            list.Add("LOAD 1 DEAD LOAD + SIDL");
            list.Add("**dEAD lOAD");
            list.Add("MEMBER LOAD");
            list.Add("153 TO 158 173 TO 178 UNI GY -2.7504");
            list.Add("151 160 171 180 UNI GY -2.66888");
            list.Add("152 159 172 179 UNI GY -1.68024");
            list.Add("133 TO 138 193 TO 198 UNI GY -2.916");
            list.Add("131 140 191 200 UNI GY -2.97768");
            list.Add("132 139 192 199 UNI GY -1.89528");
            list.Add("1 TO 10 101 TO 110 UNI GY -0.702");
            list.Add("** SIDL");
            list.Add("MEMBER LOAD");
            list.Add("** WEARING COAT");
            list.Add("131 TO 140 191 TO 200 UNI GY -0.68");
            list.Add("151 TO 160 171 TO 180 UNI GY -0.53");
            list.Add("**CRASH BARRIER");
            list.Add("111 TO 120 211 TO 220 UNI GY -1.0");
            list.Add("**** OUTER GIRDER *********");
            //list.Add("DEFINE MOVING LOAD FILE LL.TXT");
            iApp.LiveLoads.Impact_Factor(ref list, iApp.DesignStandard);
            //list.Add("TYPE 1 CLA 1.179");
            //list.Add("TYPE 2 A70R 1.188");
            //list.Add("TYPE 3 A70RT 1.10");
            //list.Add("TYPE 4 CLAR 1.179");
            //list.Add("TYPE 5 A70RR 1.188");
            //list.Add("TYPE 6 A70RR 1.188");
            //list.Add("TYPE 7 A70RR 1.188");
            //list.Add("TYPE 8 A70RR 1.188");
            //list.Add("TYPE 9 A70RR 1.188");
            //list.Add("TYPE 10 A70RR 1.188");
            //list.Add("TYPE 11 A70RR 1.188");
            //list.Add("TYPE 12 A70RR 1.188");
            //list.Add("TYPE 13 A70RR 1.188");
            //list.Add("**** 3 LANE CLASS A *****");
            list.Add("LOAD GENERATION 191");
            list.Add("TYPE 1 -18.8 0 2.75 XINC 0.2");
            list.Add("TYPE 1 -18.8 0 6.25 XINC 0.2");
            list.Add("TYPE 1 -18.8 0 9.75 XINC 0.2");
            //list.Add("**** 3 LANE CLASS A *****");
            //list.Add("*LOAD GENERATION 160");
            //list.Add("*TYPE 1 -18.8 0 2.125 XINC 0.2");
            //list.Add("*TYPE 1 -18.8 0 5.625 XINC 0.2");
            //list.Add("*TYPE 1 -18.8 0 9.125 XINC 0.2");
            //list.Add("*PLOT DISPLACEMENT FILE");
            list.Add("PRINT SUPPORT REACTIONS");
            list.Add("PRINT MAX FORCE ENVELOPE LIST " + List_Envelop_Outer);
            list.Add("PRINT MAX FORCE ENVELOPE LIST " + List_Envelop_Inner);
            list.Add("PERFORM ANALYSIS");
            list.Add("FINISH");
            list.Add(kStr);
            File.WriteAllLines(file_name, list.ToArray());
            //iApp.Write_LiveLoad_LL_TXT(Path.GetDirectoryName(file_name), true, iApp.DesignStandard);
            iApp.Write_LiveLoad_LL_TXT(Path.GetDirectoryName(file_name));
            list.Clear();
        }

        private void Set_Section_Properties(List<string> list)
        {

            list.Add(string.Format("{0} PRIS YD {1:f4} ZD 1.0",
                _DeckSlab,
                Ds));

            list.Add(string.Format("{0} PRIS YD {1:f4} ZD 1.0",
                _Cross_Girder_End,
                Ds));

            list.Add(string.Format("{0} PRIS YD {1:f4} ZD 1.0",
                _Cross_Girder_Inter,
                Ds));

            list.Add(string.Format("{0} PRIS YD {1:f4} ZD 1.0",
                _Outer_Girder_Support,
                Ds));

            list.Add(string.Format("{0} PRIS YD {1:f4} ZD 1.0",
                _Outer_Girder_Mid,
                Ds));

            list.Add(string.Format("{0} PRIS YD {1:f4} ZD 1.0",
                _Inner_Girder_Support,
                Ds));

            if (_Inner_Girder_Support.StartsWith("0") == false)
                list.Add(string.Format("{0} PRIS YD {1:f4} ZD 1.0", _Inner_Girder_Support, Ds));

            if (_Inner_Girder_Mid.StartsWith("0") == false)
                list.Add(string.Format("{0} PRIS YD {1:f4} ZD 1.0", _Inner_Girder_Mid, Ds));

            //if (_Inner_Girder_Mid.StartsWith("0") == false)
            //    list.Add(string.Format("{0} PRIS YD {1:f4} ZD 1.0", _Inner_Girder_Mid, Ds));

            //list.Add(string.Format("{0} PRIS AX {1:f4} IX {2:f4} IZ {3:f4} ",
            //    _Cross_Girder_End,
            //    Cross_End_Section.Composite_Section_A,
            //    Cross_End_Section.Composite_Section_Ix,
            //    Cross_End_Section.Composite_Section_Iz));

            //list.Add(string.Format("{0} PRIS AX {1:f4} IX {2:f4} IZ {3:f4} ",
            //    _Cross_Girder_Inter,
            //    Cross_Intermediate_Section.Composite_Section_A,
            //    Cross_Intermediate_Section.Composite_Section_Ix,
            //    Cross_Intermediate_Section.Composite_Section_Iz));

            //list.Add(string.Format("{0} PRIS AX {1:f4} IX {2:f4} IZ {3:f4} ",
            //    _Outer_Girder_Support,
            //    Long_Outer_Support_Section.Composite_Section_A,
            //    Long_Outer_Support_Section.Composite_Section_Ix,
            //    Long_Outer_Support_Section.Composite_Section_Iz));

            //list.Add(string.Format("{0} PRIS AX {1:f4} IX {2:f4} IZ {3:f4} ",
            //    _Outer_Girder_Mid,
            //    Long_Outer_Mid_Section.Composite_Section_A,
            //    Long_Outer_Mid_Section.Composite_Section_Ix,
            //    Long_Outer_Mid_Section.Composite_Section_Iz));

            //if (_Inner_Girder_Support.StartsWith("0") == false)
            //    list.Add(string.Format("{0} PRIS AX {1:f4} IX {2:f4} IZ {3:f4} ",
            //        _Inner_Girder_Support,
            //        Long_Inner_Support_Section.Composite_Section_A,
            //        Long_Inner_Support_Section.Composite_Section_Ix,
            //        Long_Inner_Support_Section.Composite_Section_Iz));

            //if (_Inner_Girder_Mid.StartsWith("0") == false)
            //    list.Add(string.Format("{0} PRIS AX {1:f4} IX {2:f4} IZ {3:f4} ",
            //    _Inner_Girder_Mid,
            //    Long_Inner_Mid_Section.Composite_Section_A,
            //    Long_Inner_Mid_Section.Composite_Section_Ix,
            //    Long_Inner_Mid_Section.Composite_Section_Iz));

            //if (HA_Loading_Members == "")
            //{
            //    list.Add(string.Format("{0} PRIS YD {1:f4} ZD 1.0",
            //        _DeckSlab,
            //        Ds));
            //}
        }
        public void WriteData_LiveLoad_Analysis(string file_name, List<string> ll_data)
        {
            string kStr = "";
            List<string> list = new List<string>();
            int i = 0;

            list.Add("ASTRA FLOOR RCC T GIRDER BRIDGE DECK ANALYSIS");
            list.Add("UNIT METER MTON");
            list.Add("JOINT COORDINATES");
            for (i = 0; i < Joints.Count; i++)
            {
                list.Add(Joints[i].ToString());
            }
            list.Add("MEMBER INCIDENCES");
            for (i = 0; i < MemColls.Count; i++)
            {
                list.Add(MemColls[i].ToString());
            }


            list.Add("SECTION PROPERTIES");

            if (Long_Inner_Mid_Section != null)
            {

                Set_Section_Properties(list);


            }
            else
            {
                list.Add(string.Format("{0} TO {1} PRIS AX 1.146 IX 0.022 IZ 0.187", MemColls[0].MemberNo, MemColls[MemColls.Count - 1].MemberNo));

            }
            list.Add("MATERIAL CONSTANT");
            list.Add("E 2.85E6 ALL");
            list.Add("DENSITY CONCRETE ALL");
            list.Add("POISSON CONCRETE ALL");
            list.Add("SUPPORT");
            //list.Add(string.Format("{0}  PINNED", support_left_joints));
            ////list.Add(string.Format("{0}  PINNED", support_right_joints));
            //list.Add(string.Format("{0}  FIXED BUT FX MZ", support_right_joints));

            list.Add(string.Format("{0}  {1}", support_left_joints, Start_Support));
            //list.Add(string.Format("{0}  PINNED", support_right_joints));
            list.Add(string.Format("{0}  {1}", support_right_joints, End_Support));


            list.Add("LOAD 1 DEAD LOAD + SIDL");
            list.Add("**DEAD lOAD");
            list.Add("MEMBER LOAD");
            list.Add("1 TO 220 UNI GY -0.0001");

            list.Add("DEFINE MOVING LOAD FILE LL.TXT");
            iApp.LiveLoads.Impact_Factor(ref list, iApp.DesignStandard);

            list.Add("LOAD GENERATION 191");
            list.Add("TYPE 1 -18.8 0 2.75 XINC 0.2");
            list.Add("TYPE 1 -18.8 0 6.25 XINC 0.2");
            list.Add("TYPE 1 -18.8 0 9.75 XINC 0.2");
            //list.Add("**** 3 LANE CLASS A *****");
            //list.Add("*LOAD GENERATION 160");
            //list.Add("*TYPE 1 -18.8 0 2.125 XINC 0.2");
            //list.Add("*TYPE 1 -18.8 0 5.625 XINC 0.2");
            //list.Add("*TYPE 1 -18.8 0 9.125 XINC 0.2");
            //list.Add("*PLOT DISPLACEMENT FILE");
            list.Add("PRINT SUPPORT REACTIONS");
            list.Add("PRINT MAX FORCE ENVELOPE LIST " + List_Envelop_Outer);
            list.Add("PRINT MAX FORCE ENVELOPE LIST " + List_Envelop_Inner);
            list.Add("PERFORM ANALYSIS");
            list.Add("FINISH");
            list.Add(kStr);
            File.WriteAllLines(file_name, list.ToArray());
            //iApp.Write_LiveLoad_LL_TXT(Path.GetDirectoryName(file_name));
            string fn = Path.GetDirectoryName(file_name);
            fn = Path.Combine(fn, "LL.TXT");
            File.WriteAllLines(fn, ll_data.ToArray());

            list.Clear();
        }

        public void WriteData_DeadLoad_Analysis(string file_name)
        {

            string kStr = "";
            List<string> list = new List<string>();
            int i = 0;

            list.Add("ASTRA FLOOR RCC T GIRDER BRIDGE DECK ANALYSIS WITH DEAD LOAD");
            list.Add("UNIT METER MTON");
            list.Add("JOINT COORDINATES");
            for (i = 0; i < Joints.Count; i++)
            {
                list.Add(Joints[i].ToString());
            }
            list.Add("MEMBER INCIDENCES");
            for (i = 0; i < MemColls.Count; i++)
            {
                list.Add(MemColls[i].ToString());
            }

            list.Add("SECTION PROPERTIES");
            if (Long_Inner_Mid_Section != null)
            {
                Set_Section_Properties(list);
            }
            else
            {
                list.Add(string.Format("{0} TO {1} PRIS AX 1.146 IX 0.022 IZ 0.187", MemColls[0].MemberNo, MemColls[MemColls.Count - 1].MemberNo));
            }
            list.Add("MATERIAL CONSTANT");
            list.Add("E 2.85E6 ALL");
            list.Add("DENSITY CONCRETE ALL");
            list.Add("POISSON CONCRETE ALL");
            list.Add("SUPPORT");
            //list.Add(string.Format("{0}  PINNED", support_left_joints));
            ////list.Add(string.Format("{0}  PINNED", support_right_joints));
            //list.Add(string.Format("{0}  FIXED BUT FX MZ", support_right_joints));


            list.Add(string.Format("{0}  {1}", support_left_joints, Start_Support));
            //list.Add(string.Format("{0}  PINNED", support_right_joints));
            list.Add(string.Format("{0}  {1}", support_right_joints, End_Support));


            //list.Add("1 3 5 7 9 11 PINNED");
            //list.Add("111 113 115 117 119 121 PINNED");
            list.Add("LOAD 1 DEAD LOAD + SIDL");
            list.Add("**DEAD lOAD");
            list.Add("MEMBER LOAD");
            list.Add("153 TO 158 173 TO 178 UNI GY -2.7504");
            list.Add("151 160 171 180 UNI GY -2.66888");
            list.Add("152 159 172 179 UNI GY -1.68024");
            list.Add("133 TO 138 193 TO 198 UNI GY -2.916");
            list.Add("131 140 191 200 UNI GY -2.97768");
            list.Add("132 139 192 199 UNI GY -1.89528");
            list.Add("1 TO 10 101 TO 110 UNI GY -0.702");
            list.Add("** SIDL");
            list.Add("MEMBER LOAD");
            list.Add("** WEARING COAT");
            list.Add("131 TO 140 191 TO 200 UNI GY -0.68");
            list.Add("151 TO 160 171 TO 180 UNI GY -0.53");
            list.Add("**CRASH BARRIER");
            list.Add("111 TO 120 211 TO 220 UNI GY -1.0");
            list.Add("**** OUTER GIRDER *********");
            //list.Add("DEFINE MOVING LOAD FILE LL.TXT");
            //iApp.LiveLoads.Impact_Factor(ref list, iApp.DesignStandard);
            //list.Add("TYPE 1 CLA 1.179");
            //list.Add("TYPE 2 A70R 1.188");
            //list.Add("TYPE 3 A70RT 1.10");
            //list.Add("TYPE 4 CLAR 1.179");
            //list.Add("TYPE 5 A70RR 1.188");
            //list.Add("TYPE 6 A70RR 1.188");
            //list.Add("TYPE 7 A70RR 1.188");
            //list.Add("TYPE 8 A70RR 1.188");
            //list.Add("TYPE 9 A70RR 1.188");
            //list.Add("TYPE 10 A70RR 1.188");
            //list.Add("TYPE 11 A70RR 1.188");
            //list.Add("TYPE 12 A70RR 1.188");
            //list.Add("TYPE 13 A70RR 1.188");
            //list.Add("**** 3 LANE CLASS A *****");
            //list.Add("LOAD GENERATION 191");
            //list.Add("TYPE 1 -18.8 0 2.75 XINC 0.2");
            //list.Add("TYPE 1 -18.8 0 6.25 XINC 0.2");
            //list.Add("TYPE 1 -18.8 0 9.75 XINC 0.2");
            //list.Add("**** 3 LANE CLASS A *****");
            //list.Add("*LOAD GENERATION 160");
            //list.Add("*TYPE 1 -18.8 0 2.125 XINC 0.2");
            //list.Add("*TYPE 1 -18.8 0 5.625 XINC 0.2");
            //list.Add("*TYPE 1 -18.8 0 9.125 XINC 0.2");
            //list.Add("*PLOT DISPLACEMENT FILE");
            list.Add("PRINT SUPPORT REACTIONS");
            list.Add("PRINT MAX FORCE ENVELOPE LIST " + List_Envelop_Outer);
            list.Add("PRINT MAX FORCE ENVELOPE LIST " + List_Envelop_Inner);
            list.Add("PERFORM ANALYSIS");
            list.Add("FINISH");
            list.Add(kStr);
            File.WriteAllLines(file_name, list.ToArray());
            //iApp.Write_LiveLoad_LL_TXT(Working_Folder, true, iApp.DesignStandard);
            list.Clear();
        }

        public void Clear()
        {
            Joints_Array = null;
            Long_Girder_Members_Array = null;
            Cross_Girder_Members_Array = null;
            MemColls.Clear();
            MemColls = null;
        }
        public void LoadReadFromGrid(DataGridView dgv_live_load)
        {
            LoadData ld = new LoadData();
            int i = 0;
            LoadList_1 = new List<LoadData>();
            //LoadList.Clear();
            MyList mlist = null;
            for (i = 0; i < dgv_live_load.RowCount; i++)
            {
                try
                {
                    ld = new LoadData();
                    mlist = new MyList(MyList.RemoveAllSpaces(dgv_live_load[0, i].Value.ToString().ToUpper()), ':');
                    ld.TypeNo = mlist.StringList[0];
                    ld.Code = mlist.StringList[1];
                    ld.X = MyList.StringToDouble(dgv_live_load[1, i].Value.ToString(), -60.0);
                    ld.Y = MyList.StringToDouble(dgv_live_load[2, i].Value.ToString(), 0.0);
                    ld.Z = MyList.StringToDouble(dgv_live_load[3, i].Value.ToString(), 1.0);
                    for (int j = 0; j < Live_Load_List.Count; j++)
                    {
                        if (Live_Load_List[j].TypeNo == ld.TypeNo)
                        {
                            ld.LoadWidth = Live_Load_List[j].LoadWidth;
                            break;
                        }
                    }
                    if ((ld.Z + ld.LoadWidth) > WidthBridge)
                    {
                        throw new Exception("Width of Bridge Deck is insufficient to accommodate \ngiven numbers of Lanes of Vehicle Load. \n\nBridge Width = " + WidthBridge + " <  Load Width (" + ld.Z + " + " + ld.LoadWidth + ") = " + (ld.Z + ld.LoadWidth));
                    }
                    else
                    {
                        ld.XINC = MyList.StringToDouble(dgv_live_load[4, i].Value.ToString(), 0.5);
                        ld.ImpactFactor = MyList.StringToDouble(dgv_live_load[5, i].Value.ToString(), 0.5);
                        LoadList_1.Add(ld);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "ASTRA", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            #region Set Loads

            List<LoadData> LoadList_tmp = new List<LoadData>(LoadList_1.ToArray());

            LoadList_2 = new List<LoadData>();
            LoadList_3 = new List<LoadData>();
            LoadList_4 = new List<LoadData>();
            LoadList_5 = new List<LoadData>();
            LoadList_6 = new List<LoadData>();

            LoadList_1.Clear();

            //70 R Wheel
            ld = new LoadData(Live_Load_List[2]);

            ld.X = ld.Distance;
            ld.Y = LoadList_tmp[0].Y;
            ld.Z = LoadList_tmp[0].Z;
            ld.XINC = LoadList_tmp[0].XINC;

            LoadList_1.Add(ld);


            //1 Lane Class A
            ld = new LoadData(Live_Load_List[0]);

            ld.X = ld.Distance;
            ld.Y = LoadList_tmp[0].Y;
            ld.Z = LoadList_tmp[0].Z;
            ld.XINC = LoadList_tmp[0].XINC;

            LoadList_2.Add(ld);



            //2 Lane Class A
            ld = new LoadData(Live_Load_List[0]);
            ld.X = ld.Distance;
            ld.Y = LoadList_tmp[0].Y;
            ld.Z = LoadList_tmp[0].Z;
            ld.XINC = LoadList_tmp[0].XINC;

            LoadList_3.Add(ld);

            ld = new LoadData(Live_Load_List[0]);
            ld.X = ld.Distance;
            ld.Y = LoadList_tmp[0].Y;
            ld.Z = LoadList_tmp[1].Z;
            ld.XINC = LoadList_tmp[0].XINC;

            LoadList_3.Add(ld);



            //3 Lane Class A
            ld = new LoadData(Live_Load_List[0]);
            ld.X = ld.Distance;
            ld.Y = LoadList_tmp[0].Y;
            ld.Z = LoadList_tmp[0].Z;
            ld.XINC = LoadList_tmp[0].XINC;

            LoadList_4.Add(ld);

            ld = new LoadData(Live_Load_List[0]);
            ld.X = ld.Distance;
            ld.Y = LoadList_tmp[0].Y;
            ld.Z = LoadList_tmp[1].Z;
            ld.XINC = LoadList_tmp[0].XINC;

            LoadList_4.Add(ld);

            ld = new LoadData(Live_Load_List[0]);
            ld.X = ld.Distance;
            ld.Y = LoadList_tmp[0].Y;
            ld.Z = LoadList_tmp[2].Z;
            ld.XINC = LoadList_tmp[0].XINC;

            LoadList_4.Add(ld);


            //1 Lane Class A + 70 RW
            ld = new LoadData(Live_Load_List[0]);
            ld.X = ld.Distance;
            ld.Y = LoadList_tmp[0].Y;
            ld.Z = LoadList_tmp[0].Z;
            ld.XINC = LoadList_tmp[0].XINC;

            LoadList_5.Add(ld);

            ld = new LoadData(Live_Load_List[2]);
            ld.X = ld.Distance;
            ld.Y = LoadList_tmp[0].Y;
            ld.Z = LoadList_tmp[1].Z;
            ld.XINC = LoadList_tmp[0].XINC;

            LoadList_5.Add(ld);



            //70 RW + 1 Lane Class A 
            ld = new LoadData(Live_Load_List[2]);
            ld.X = ld.Distance;
            ld.Y = LoadList_tmp[0].Y;
            ld.Z = LoadList_tmp[0].Z;
            ld.XINC = LoadList_tmp[0].XINC;

            LoadList_6.Add(ld);

            ld = new LoadData(Live_Load_List[0]);
            ld.X = ld.Distance;
            ld.Y = LoadList_tmp[0].Y;
            ld.Z = LoadList_tmp[1].Z;
            ld.XINC = LoadList_tmp[0].XINC;

            LoadList_6.Add(ld);

            #endregion Set Loads

        }
    }


}
