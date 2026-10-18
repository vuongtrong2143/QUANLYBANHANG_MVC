using System;

namespace QUANLYBANHANG_MVC.Models
{
    [Serializable]
    public class CartItem
    {
        public int MASANPHAM { get; set; }

        public string TENSANPHAM { get; set; }

        public string HINHANH { get; set; }

        public decimal DONGIA { get; set; }

        public int SOLUONG { get; set; }

        public decimal THANHTIEN
        {
            get { return DONGIA * SOLUONG; }
        }
    }
}