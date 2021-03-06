﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils.Linq2Sql;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Excel;
using DataTable = System.Data.DataTable;
using System.Globalization;
using MetroFramework.Forms;

namespace Admin
{
    public partial class frmQuanLy : MetroForm
    {

        public EventHandler swapform;
        int idadmin;
        public frmQuanLy(int id)
        {
            InitializeComponent();
            btnHuy.Click += (s, e) => { Close(); };
            FormClosed += (s, e) => { swapform?.Invoke(null, null); };
            idadmin = id;
            cbbPer.SelectedIndexChanged += cbbPer_SelectedIndexChanged;
            btnadduser.Click += btnadduser_Click;
            btnEdituser.Click += Button1_Click;
            btnImport.Click += btnImport_Click;
            btnExport.Click += btnExport_Click;
        }

        private void frmQuanLy_Load(object sender, EventArgs e)
        {
            cbbPer.SelectedIndex = 3;
            load(-1);
            if (idadmin == 0)
            {
                label3.Text = "Admin";
            }
            else
            {
                using (var qltn = Utils.QLTN.getInstance())
                {
                    var u = qltn.TaiKhoans.Where(x => x.id == idadmin).FirstOrDefault();
                    label3.Text = u.hoten;
                }
            }
        }
        private void load(int permission)
        {
            using (var qltn = Utils.QLTN.getInstance())
            {
                var ds = qltn.TaiKhoans.Where(x =>
                    permission < 0 ? true : x.permission == permission
                ).Select(x => new
                {
                    x.id,
                    x.tentaikhoan,
                    x.matkhau,
                    x.hoten,
                    ngaysinh = x.ngaysinh.Value,
                    quyenhan = x.permission.Value == 0 ? "Học sinh" : (x.permission.Value == 1 ? "Giao viên" : "Admin"),
                    lophoc = x.LopHoc != null ? x.LopHoc.tenlop : ""
                });
                dgvDS.DataSource = ds.ToList();
            }
        }
        private void cbbPer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbbPer.SelectedIndex == 0)
            {
                load(0);
            }
            else if (cbbPer.SelectedIndex == 1)
            {
                load(1);
            }
            else if (cbbPer.SelectedIndex == 2)
            {
                load(2);
            }
            else if (cbbPer.SelectedIndex == 3)
            {
                load(-1);
            }
        }

        private void btnadduser_Click(object sender, EventArgs e)
        {
            frmAdduser adduser = new frmAdduser();
            adduser.swapform += (s, e1) => { Show(); frmQuanLy_Load(s, e1); };
            adduser.Show();
            Hide();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            frmEdituser edituser = new frmEdituser();
            edituser.tk = dgvDS.SelectedRows[0].Cells["tentaikhoan"].Value.ToString();
            edituser.swapform += (s, e1) => { Show(); frmQuanLy_Load(s, e1); };
            edituser.Show();
            Hide();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {

            string saveExcelFile = "";
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Excel 2010|*.xlsx";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                saveExcelFile = dlg.FileName;
            }
            else
                return;

            Excel.Application xlApp = new Excel.Application();

            xlApp.Interactive = false;

            if (xlApp == null)
            {
                MessageBox.Show("Lỗi không thể sử dụng được thư viện EXCEL");
                xlApp.Quit();
                return;
            }
            int permission = cbbPer.SelectedIndex;

            new Utils.frmWaiting(() =>
            {
                try
                {

                    xlApp.Visible = false;

                    object misValue = System.Reflection.Missing.Value;

                    Workbook wb = xlApp.Workbooks.Add(misValue);

                    Worksheet ws = (Worksheet)wb.Worksheets[1];

                    if (ws == null)
                    {
                        MessageBox.Show("Không thể tạo được WorkSheet");
                        return;
                    }

                    ws.Cells.NumberFormat = "@";
                    int row = 1;
                    string fontName = "Times New Roman";
                    int fontSizeTieuDe = 18;
                    int fontSizeTenTruong = 14;
                    int fontSizeNoiDung = 12;
                    //Xuất dòng Tiêu đề của File báo cáo: Lưu ý
                    Range row1_DSTK = ws.get_Range("A1", "E1");
                    row1_DSTK.Merge();
                    row1_DSTK.Font.Size = fontSizeTieuDe;
                    row1_DSTK.Font.Name = fontName;
                    row1_DSTK.Cells.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    row1_DSTK.Value2 = "Danh sách tài khoản";

                    //Tạo Ô ID
                    Range row23_ID = ws.get_Range("A2", "A3");//Cột A dòng 2 và dòng 3
                    row23_ID.Merge();
                    row23_ID.Font.Size = fontSizeTenTruong;
                    row23_ID.Font.Name = fontName;
                    row23_ID.Cells.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    row23_ID.Value2 = "ID";

                    //Tạo Ô Tên tài khoản
                    Range row23_TenTK = ws.get_Range("B2", "B3");//Cột B dòng 2 và dòng 3
                    row23_TenTK.Merge();
                    row23_TenTK.Font.Size = fontSizeTenTruong;
                    row23_TenTK.Font.Name = fontName;
                    row23_TenTK.Cells.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    row23_TenTK.Value2 = "Tên tài khoản";
                    row23_TenTK.ColumnWidth = 20;

                    //Tạo Ô Mật khẩu
                    Range row23_MatKhau = ws.get_Range("C2", "C3");//Cột C dòng 2 và dòng 3
                    row23_MatKhau.Merge();
                    row23_MatKhau.Font.Size = fontSizeTenTruong;
                    row23_MatKhau.Font.Name = fontName;
                    row23_MatKhau.Cells.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    row23_MatKhau.ColumnWidth = 20;
                    row23_MatKhau.Value2 = "Mật khẩu";

                    //Tạo Ô Họ tên
                    Range row23_Hoten = ws.get_Range("D2", "D3");//Cột D dong 2 3
                    row23_Hoten.Merge();
                    row23_Hoten.Font.Size = fontSizeTenTruong;
                    row23_Hoten.Font.Name = fontName;
                    row23_Hoten.Cells.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    row23_Hoten.ColumnWidth = 20;
                    row23_Hoten.Value2 = "Họ và Tên";

                    //O Ngsinh
                    Range row23_Ngsinh = ws.get_Range("E2", "E3");
                    row23_Ngsinh.Merge();
                    row23_Ngsinh.Font.Size = fontSizeTenTruong;
                    row23_Ngsinh.Font.Name = fontName;
                    row23_Ngsinh.Cells.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    row23_Ngsinh.ColumnWidth = 20;
                    row23_Ngsinh.Value2 = "Ngay Sinh";

                    // Quyen
                    Range row23_Per = ws.get_Range("F2", "F3");
                    row23_Per.Merge();
                    row23_Per.Font.Size = fontSizeTenTruong;
                    row23_Per.Font.Name = fontName;
                    row23_Per.Cells.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    row23_Per.ColumnWidth = 20;
                    row23_Per.Value2 = "Quyen";

                    //ClassID
                    Range row23_classID = ws.get_Range("G2", "G3");
                    row23_classID.Merge();
                    row23_classID.Font.Size = fontSizeTenTruong;
                    row23_classID.Font.Name = fontName;
                    row23_classID.Cells.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    row23_classID.ColumnWidth = 20;
                    row23_classID.Value2 = "Lop hoc ID";

                    //
                    //Tô nền vàng các cột tiêu đề:
                    Range row23_CotTieuDe = ws.get_Range("A2", "G3");
                    //nền vàng
                    row23_CotTieuDe.Interior.Color = ColorTranslator.ToOle(System.Drawing.Color.Yellow);
                    //in đậm
                    row23_CotTieuDe.Font.Bold = true;
                    //chữ đen
                    row23_CotTieuDe.Font.Color = ColorTranslator.ToOle(System.Drawing.Color.Black);

                    row = 3;//dữ liệu xuất bắt đầu từ dòng số 4 trong file Excel (khai báo 3 để vào vòng lặp nó ++ thành 4)

                    if (permission == 3) permission = -1;

                    using (var qltn = Utils.QLTN.getInstance())
                    {
                        List<TaiKhoan> tk = qltn.TaiKhoans.Where(x =>
                                 permission < 0 ? true : x.permission == permission
                            ).ToList();

                        foreach (TaiKhoan tk1 in tk)
                        {
                            row++;
                            dynamic[] arr = { tk1.id, tk1.tentaikhoan, tk1.matkhau, tk1.hoten, tk1.ngaysinh.Value.ToString(), tk1.permission, tk1.lophocid };
                            Range rowData = ws.get_Range("A" + row, "G" + row);//Lấy dòng thứ row ra để đổ dữ liệu
                            rowData.Font.Size = fontSizeNoiDung;
                            rowData.Font.Name = fontName;
                            rowData.Value2 = arr;
                        }
                    }



                    //Kẻ khung toàn bộ
                    BorderAround(ws.get_Range("A2", "G" + row));

                    //Lưu file excel xuống Ổ cứng
                    wb.SaveAs(saveExcelFile);

                    //đóng file để hoàn tất quá trình lưu trữ
                    wb.Close(true, misValue, misValue);


                    MessageBox.Show("Đã xuất dữ liệu thành công tại: " + saveExcelFile);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    xlApp.Quit();
                }
            }).ShowDialog();
        }
        private void BorderAround(Range range)
        {
            Borders borders = range.Borders;
            borders[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
            borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
            borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
            borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
            borders.Color = Color.Black;
            borders[XlBordersIndex.xlInsideVertical].LineStyle = XlLineStyle.xlContinuous;
            borders[XlBordersIndex.xlInsideHorizontal].LineStyle = XlLineStyle.xlContinuous;
            borders[XlBordersIndex.xlDiagonalUp].LineStyle = XlLineStyle.xlLineStyleNone;
            borders[XlBordersIndex.xlDiagonalDown].LineStyle = XlLineStyle.xlLineStyleNone;
        }
        //Hàm thu hồi bộ nhớ cho COM Excel
        private static void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                obj = null;
            }
            finally
            { GC.Collect(); }
        }
        private bool check(string tentk)
        {
            using (var qltn = Utils.QLTN.getInstance())
            {
                var u = qltn.TaiKhoans.Where(x => x.tentaikhoan == tentk).FirstOrDefault();
                if ((u.permission != 2 && u.lophocid == null) || (u.permission == 2 && u.lophocid != null))
                {
                    return false;
                }
                else
                    return true;
            }
        }
        private bool checkpermission(int per, int lhid)
        {
            if ((per == 1 || per == 0) && lhid != 0)
            {
                return true;
            }
            else if (per == 2 && lhid != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Excel 2010|*.xlsx";
            string duongdan = "";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                duongdan = dlg.FileName;
            }
            else
                return;

            var xlApp = new Excel.Application();

            if (xlApp == null)
            {
                MessageBox.Show("Thư viện excel chưa được cài đặt");
                xlApp.Quit();
                return;
            }

            new Utils.frmWaiting(() =>
            {

                try
                {
                    xlApp.Visible = false;

                    var workbook = xlApp.Workbooks.Open(duongdan);

                    Excel.Worksheet worksheet = workbook.Worksheets[1];
                    int lastrow = worksheet.UsedRange.Rows.Count;


                    using (var qltn = Utils.QLTN.getInstance())
                    {
                        for (int i = 4; i <= lastrow; i++)
                        {
                            int? Lophoc = null;

                            int col = 2;

                            var ch = new TaiKhoan
                            {
                                tentaikhoan = worksheet.Cells[i, col++].Text,
                                matkhau = worksheet.Cells[i, col++].Text,
                                hoten = worksheet.Cells[i, col++].Text,
                                ngaysinh = DateTime.Parse(worksheet.Cells[i, col++].Text),
                                permission = int.Parse(worksheet.Cells[i, col].Text),
                                lophocid = int.Parse(worksheet.Cells[i, col++].Text) == 2 ? Lophoc : int.Parse(worksheet.Cells[i, col].Text),
                            };
                            qltn.TaiKhoans.InsertOnSubmit(ch);
                        }
                        qltn.SubmitChanges();

                    }
                    MessageBox.Show("Đã import thành công");
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.ToString()); }
                finally
                {
                    xlApp.Quit();
                }
            }).ShowDialog();
            load(-1);
        }

    }
}

