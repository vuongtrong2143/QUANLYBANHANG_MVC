using System;
using System.Collections.Generic;
using System.Linq;

namespace QUANLYBANHANG_MVC.Models
{
    [Serializable]
    public class CART
    {
        public Dictionary<int, CartItem> LISTCARTS { get; private set; }

        public CART()
        {
            LISTCARTS = new Dictionary<int, CartItem>();
        }

        public void AddCart(
            int maSanPham,
            string tenSanPham,
            string hinhAnh,
            int soLuong,
            decimal donGia)
        {
            if (soLuong <= 0)
            {
                throw new ArgumentOutOfRangeException("soLuong");
            }

            CartItem item;

            if (LISTCARTS.TryGetValue(maSanPham, out item))
            {
                // Đã có sản phẩm: tăng số lượng.
                item.SOLUONG += soLuong;

                // Đồng bộ thông tin từ dữ liệu sản phẩm hiện tại.
                item.TENSANPHAM = tenSanPham;
                item.HINHANH = hinhAnh;
                item.DONGIA = donGia;
            }
            else
            {
                // Chưa có sản phẩm: thêm dòng mới.
                LISTCARTS.Add(
                    maSanPham,
                    new CartItem
                    {
                        MASANPHAM = maSanPham,
                        TENSANPHAM = tenSanPham,
                        HINHANH = hinhAnh,
                        SOLUONG = soLuong,
                        DONGIA = donGia
                    }
                );
            }
        }

        public void RemoveCart(int maSanPham)
        {
            LISTCARTS.Remove(maSanPham);
        }

        public decimal TotalBill()
        {
            return LISTCARTS.Values.Sum(item => item.THANHTIEN);
        }

        public int TotalQuantity()
        {
            return LISTCARTS.Values.Sum(item => item.SOLUONG);
        }
    }
}