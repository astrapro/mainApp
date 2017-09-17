﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using AstraInterface.DataStructure;
using AstraInterface.Interface;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
namespace BridgeAnalysisDesign.Pier
{
    public partial class UC_PierPileLS : UserControl
    {
        public UC_PierPileLS()
        {
            InitializeComponent();
        }

        IApplication iApp = null;
        public event EventHandler OnProcess;

        public void SetIApplocation(IApplication iApp)
        {
            this.iApp = iApp;
        }
        string user_path = "";

        public string Title
        {
            get
            {
                //if (iApp.DesignStandard == eDesignStandard.BritishStandard)
                //    return "DESIGN OF RCC ABUTMENT CANTILEVER [BS]";
                return "Pier Design with Pile Foundation in LSM";
            }
        }

        public bool Show_Title
        {
            get
            {
                return lbl_Title.Visible;
            }
            set
            {
                lbl_Title.Visible = value;
            }
        }

        private void lbl_1_DoubleClick(object sender, EventArgs e)
        {
            Label lbl = sender as Label;

            Panel pnl = pnl_1;


            if (lbl == lbl_1) pnl = pnl_1;
            else if (lbl == lbl_2) pnl = pnl_2;
            else if (lbl == lbl_3) pnl = pnl_3;
            else if (lbl == lbl_4) pnl = pnl_4;
            else if (lbl == lbl_5) pnl = pnl_5;
            else if (lbl == lbl_6) pnl = pnl_6;
            else if (lbl == lbl_7) pnl = pnl_7;
            //else if (lbl == lbl_8) pnl = pnl_8;

            pnl.Visible = !pnl.Visible;

            if (pnl.Visible) lbl.ForeColor = Color.Black;
            else lbl.ForeColor = Color.Blue;

        }

        private void btn_proceed_Click(object sender, EventArgs e)
        {

            MessageBox.Show(this, "The Design in Excel Worksheet will take some time to complete. Please wait until the process is complete as shown at the bottom of the Excel Worksheet.", "ASTRA", MessageBoxButtons.OK);

            if (iApp.DesignStandard == eDesignStandard.IndianStandard)
            {
                Pier_Process_Design_IS();
            }
            else
            {
                Pier_Process_Design_BS();
            }
            if (OnProcess != null) OnProcess(sender, e);

        }

        private void Pier_Process_Design_IS()
        {

            string file_path = Path.Combine(iApp.LastDesignWorkingFolder, Title);
            if (iApp.user_path != "")
                file_path = Path.Combine(iApp.user_path, Title);

            if (!Directory.Exists(file_path)) Directory.CreateDirectory(file_path);

            //file_path = Path.Combine(file_path, "RCC Cantilever Abutment Design");

            //if (!Directory.Exists(file_path)) Directory.CreateDirectory(file_path);

            file_path = Path.Combine(file_path, "Pier with Pile Foundation.xlsm");

            //file_path = Path.Combine(file_path, "BoQ_Flyover_ROB_RUBs.xlsx");
            //file_path = Path.Combine(file_path, "BoQ for " + cmb_boq_item.Text + ".xlsx");

            string copy_path = file_path;

            file_path = Path.Combine(Application.StartupPath, @"DESIGN\Pier\Pier Design Limit State\Pier with Pile Foundation.xlsm");

            if (File.Exists(file_path))
            {
                File.Copy(file_path, copy_path, true);
            }
            else
            {
                MessageBox.Show(file_path + " file not found.");
                return;
            }


            iApp.Excel_Open_Message();

            Excel.Application myExcelApp;
            Excel.Workbooks myExcelWorkbooks;
            Excel.Workbook myExcelWorkbook;

            object misValue = System.Reflection.Missing.Value;

            myExcelApp = new Excel.ApplicationClass();
            myExcelApp.Visible = true;
            //myExcelApp.Visible = false;
            myExcelWorkbooks = myExcelApp.Workbooks;

            //myExcelWorkbook = myExcelWorkbooks.Open(fileName, misValue, misValue, misValue, misValue, misValue, misValue, misValue, misValue, misValue, misValue, misValue, misValue, misValue, misValue);

            myExcelWorkbook = myExcelWorkbooks.Open(copy_path, 0, false, 5, "2011ap", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

            //Excel.Worksheet myExcelWorksheet = (Excel.Worksheet)myExcelWorkbook.ActiveSheet;
            Excel.Worksheet myExcelWorksheet = (Excel.Worksheet)myExcelWorkbook.Sheets["1. Lvl & Dim."];


            List<TextBox> All_Data = Get_TextBoxes();


            //Excel.Range formatRange;
            //formatRange = myExcelWorksheet.get_Range("b" + (dgv.RowCount + after_indx), "L" + (dgv.RowCount + after_indx));
            //formatRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGreen);


            List<double> data = new List<double>();
            try
            {
                string kStr = "";
                //foreach (var item in All_Data)
                //{
                //    kStr = item.Name.Replace("txt_des_", "");

                //    //myExcelWorksheet.get_Range("E53").Formula = data[rindx++].ToString();
                //    myExcelWorksheet.get_Range(kStr).Formula = item.Text;
                //}

                #region Input 2

                myExcelWorksheet = (Excel.Worksheet)myExcelWorkbook.Sheets["2.Design Paramters"];


                //myExcelWorksheet.get_Range("K78").Formula = data[rindx++].ToString();
                //myExcelWorksheet.get_Range("K81").Formula = data[rindx++].ToString();

                #endregion Input 2


            }
            catch (Exception exx) { }

            myExcelWorkbook.Save();

            releaseObject(myExcelWorkbook);

            //iApp.Excel_Open_Message();
        }

        public List<TextBox> Get_TextBoxes()
        {
            List<TextBox> list = new List<TextBox>();
            list.Add(txt_xls_des_F61);
            list.Add(txt_xls_des_F62);
            list.Add(txt_xls_des_G12);
            list.Add(txt_xls_des_G14);
            list.Add(txt_xls_des_G15);
            list.Add(txt_xls_des_G16);
            list.Add(txt_xls_des_G17);
            list.Add(txt_xls_des_G18);
            list.Add(txt_xls_des_G34);
            list.Add(txt_xls_des_G35);
            list.Add(txt_xls_des_G4);
            list.Add(txt_xls_des_G63);
            list.Add(txt_xls_des_G64);
            list.Add(txt_xls_des_G66);
            list.Add(txt_xls_des_H61);
            list.Add(txt_xls_des_H62);
            list.Add(txt_xls_des_J61);
            list.Add(txt_xls_des_J62);


            list.Add(txt_xls_inp_A122);
            list.Add(txt_xls_inp_A153);
            list.Add(txt_xls_inp_D16);
            list.Add(txt_xls_inp_D17);
            list.Add(txt_xls_inp_D18);
            list.Add(txt_xls_inp_E115);
            list.Add(txt_xls_inp_E120);
            list.Add(txt_xls_inp_E128);
            list.Add(txt_xls_inp_E4);
            list.Add(txt_xls_inp_E88);
            list.Add(txt_xls_inp_G148);
            list.Add(txt_xls_inp_G15);
            list.Add(txt_xls_inp_G16);
            list.Add(txt_xls_inp_G17);
            list.Add(txt_xls_inp_G18);
            list.Add(txt_xls_inp_G19);
            list.Add(txt_xls_inp_G20);
            list.Add(txt_xls_inp_G21);
            list.Add(txt_xls_inp_G22);
            list.Add(txt_xls_inp_G23);
            list.Add(txt_xls_inp_G24);
            list.Add(txt_xls_inp_G25);
            list.Add(txt_xls_inp_G26);
            list.Add(txt_xls_inp_G28);
            list.Add(txt_xls_inp_G29);
            list.Add(txt_xls_inp_G30);
            list.Add(txt_xls_inp_G31);
            list.Add(txt_xls_inp_G7);
            list.Add(txt_xls_inp_G8);
            list.Add(txt_xls_inp_G9);
            list.Add(txt_xls_inp_H116);
            list.Add(txt_xls_inp_H7);
            list.Add(txt_xls_inp_H8);
            list.Add(txt_xls_inp_H82);
            list.Add(txt_xls_inp_H85);
            list.Add(txt_xls_inp_H9);
            list.Add(txt_xls_inp_I103);
            list.Add(txt_xls_inp_I109);
            list.Add(txt_xls_inp_I122);
            list.Add(txt_xls_inp_I15);
            list.Add(txt_xls_inp_I16);
            list.Add(txt_xls_inp_I17);
            list.Add(txt_xls_inp_I18);
            list.Add(txt_xls_inp_I19);
            list.Add(txt_xls_inp_I20);
            list.Add(txt_xls_inp_I21);
            list.Add(txt_xls_inp_I22);
            list.Add(txt_xls_inp_I23);
            list.Add(txt_xls_inp_I24);
            list.Add(txt_xls_inp_I25);
            list.Add(txt_xls_inp_I26);
            list.Add(txt_xls_inp_I28);
            list.Add(txt_xls_inp_I29);
            list.Add(txt_xls_inp_I30);
            list.Add(txt_xls_inp_I31);
            list.Add(txt_xls_inp_I7);
            list.Add(txt_xls_inp_I8);
            list.Add(txt_xls_inp_I9);
            list.Add(txt_xls_inp_I97);
            list.Add(txt_xls_inp_J196);
            list.Add(txt_xls_inp_J7);
            list.Add(txt_xls_inp_J8);
            list.Add(txt_xls_inp_J9);
            list.Add(txt_xls_inp_K115);
            list.Add(txt_xls_inp_K70);
            return list;
        }
        private void Pier_Process_Design_BS()
        {

            string file_path = Path.Combine(iApp.LastDesignWorkingFolder, Title);
            if (iApp.user_path != "")
                file_path = Path.Combine(iApp.user_path, Title);

            if (!Directory.Exists(file_path)) Directory.CreateDirectory(file_path);

            //file_path = Path.Combine(file_path, "RCC Cantilever Abutment Design");

            //if (!Directory.Exists(file_path)) Directory.CreateDirectory(file_path);

            file_path = Path.Combine(file_path, "Pier with open foundation.xlsx");

            //file_path = Path.Combine(file_path, "BoQ_Flyover_ROB_RUBs.xlsx");
            //file_path = Path.Combine(file_path, "BoQ for " + cmb_boq_item.Text + ".xlsx");

            string copy_path = file_path;

            file_path = Path.Combine(Application.StartupPath, @"DESIGN\Pier\Pier Design Limit State\Pier with open foundation BS.xlsx");

            if (File.Exists(file_path))
            {
                File.Copy(file_path, copy_path, true);
            }
            else
            {
                MessageBox.Show(file_path + " file not found.");
                return;
            }


            iApp.Excel_Open_Message();

            Excel.Application myExcelApp;
            Excel.Workbooks myExcelWorkbooks;
            Excel.Workbook myExcelWorkbook;

            object misValue = System.Reflection.Missing.Value;

            myExcelApp = new Excel.ApplicationClass();
            myExcelApp.Visible = true;
            //myExcelApp.Visible = false;
            myExcelWorkbooks = myExcelApp.Workbooks;

            //myExcelWorkbook = myExcelWorkbooks.Open(fileName, misValue, misValue, misValue, misValue, misValue, misValue, misValue, misValue, misValue, misValue, misValue, misValue, misValue, misValue);

            myExcelWorkbook = myExcelWorkbooks.Open(copy_path, 0, false, 5, "2011ap", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

            //Excel.Worksheet myExcelWorksheet = (Excel.Worksheet)myExcelWorkbook.ActiveSheet;
            Excel.Worksheet myExcelWorksheet = (Excel.Worksheet)myExcelWorkbook.Sheets["Design Data"];


            List<TextBox> All_Data = Get_TextBoxes();


            //Excel.Range formatRange;
            //formatRange = myExcelWorksheet.get_Range("b" + (dgv.RowCount + after_indx), "L" + (dgv.RowCount + after_indx));
            //formatRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGreen);


            List<double> data = new List<double>();
            try
            {
                string kStr = "";
                //foreach (var item in All_Data)
                //{
                //    if (item.Name.Contains("txt_xls_inp_", ""))
                //    {
                //        kStr = item.Name.Replace("txt_xls_inp_", "");

                //        //myExcelWorksheet.get_Range("E53").Formula = data[rindx++].ToString();
                //        myExcelWorksheet.get_Range(kStr).Formula = item.Text;
                //    }
                //}

                #region Input 2

                //myExcelWorksheet.get_Range("K78").Formula = data[rindx++].ToString();
                //myExcelWorksheet.get_Range("K81").Formula = data[rindx++].ToString();

                #endregion Input 2


            }
            catch (Exception exx) { }

            myExcelWorkbook.Save();

            releaseObject(myExcelWorkbook);

            //iApp.Excel_Open_Message();
        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show("Exception Occured while releasing object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }

        private void btn_open_Click(object sender, EventArgs e)
        {
            iApp.Open_ASTRA_Worksheet_Dialog();
        }
    }
}
