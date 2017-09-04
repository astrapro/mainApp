﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AstraAccess.SAP_Forms
{
    public partial class frm_Beam_Mat_Props : Form
    {

        public DataGridView DGV { get; set; }

        public bool IsEdit { get; set; }
        public int row_index
        {
            get
            {
                if (DGV.SelectedCells.Count > 0)
                    return DGV.SelectedCells[0].RowIndex;
                return -1;
            }
        }

        public frm_Beam_Mat_Props(DataGridView dgv, bool isEdit)
        {
            InitializeComponent();
            DGV = dgv;
            IsEdit = isEdit;
        }

        private void frm_Beam_Mat_Props_Load(object sender, EventArgs e)
        {
            if(IsEdit)
            {
                btn_add.Text = "Change";

                int c = 0;
                int r = row_index;
                if(r != -1)
                {
                    txt_mat_no.Text = DGV[c++, r].Value.ToString();
                    txt_young_mod.Text = DGV[c++, r].Value.ToString();
                    txt_pr.Text = DGV[c++, r].Value.ToString();
                    txt_mass_den.Text = DGV[c++, r].Value.ToString();
                    txt_wgt_den.Text = DGV[c++, r].Value.ToString();
                }
            }
            else
            {
                txt_mat_no.Text = (DGV.RowCount + 1).ToString();
            }
        }

        private void btn_add_mem_Click(object sender, EventArgs e)
        {
            if (IsEdit)
            {
                int c = 0;
                int r = row_index;

                DGV[c++, r].Value = txt_mat_no.Text;
                DGV[c++, r].Value = txt_young_mod.Text;
                DGV[c++, r].Value = txt_pr.Text;
                DGV[c++, r].Value = txt_mass_den.Text;
                DGV[c++, r].Value = txt_wgt_den.Text;

                this.Close();
       
            }
            else
            {

                DGV.Rows.Add(txt_mat_no.Text,
                    txt_young_mod.Text,
                    txt_pr.Text,
                    txt_mass_den.Text,
                    txt_wgt_den.Text
                    );
                txt_mat_no.Text = (DGV.RowCount + 1).ToString();
            }
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
