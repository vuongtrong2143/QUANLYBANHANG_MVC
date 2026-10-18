using QUANLYBANHANG_MVC.Models;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace QUANLYBANHANG_MVC.Controllers
{
    public class GioHangController : Controller
    {
        private readonly dbQUANLYBANHANGEntities db =
            new dbQUANLYBANHANGEntities();

        private CART GetCart()
        {
            CART cart = Session["CART"] as CART;

            if (cart == null)
            {
                cart = new CART();
                Session["CART"] = cart;
            }

            return cart;
        }

        // GET: /GioHang/Index
        [HttpGet]
        public ActionResult Index()
        {
            CART cart = GetCart();

            // Danh mục hiển thị ở cột bên trái.
            ViewBag.DanhMuc = db.tbDANHMUCs.ToList();

            return View(cart);
        }

        // POST: /GioHang/ThemVaoGio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemVaoGio(int? id, int soLuong = 1)
        {
            if (!id.HasValue || soLuong <= 0)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest
                );
            }

            // Lấy thông tin và giá từ cơ sở dữ liệu,
            // không nhận giá do trình duyệt gửi lên.
            tbSANPHAM sanPham = db.tbSANPHAMs.Find(id.Value);

            if (sanPham == null)
            {
                return HttpNotFound();
            }

            // Không thêm sản phẩm chưa có giá.
            if ((object)sanPham.DONGIA == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest,
                    "San pham chua co don gia."
                );
            }

            decimal donGia = Convert.ToDecimal(sanPham.DONGIA);

            if (donGia < 0)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest,
                    "Don gia khong hop le."
                );
            }

            CART cart = GetCart();

            cart.AddCart(
                sanPham.MASANPHAM,
                sanPham.TENSANPHAM,
                sanPham.HINHANH,
                soLuong,
                donGia
            );

            Session["CART"] = cart;

            TempData["ThongBao"] =
                "Đã thêm sản phẩm vào giỏ hàng.";

            return RedirectToAction("Index");
        }

        // POST: /GioHang/XoaDaChon
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XoaDaChon(int[] maSanPham)
        {
            CART cart = GetCart();

            if (maSanPham != null && maSanPham.Length > 0)
            {
                foreach (int ma in maSanPham)
                {
                    cart.RemoveCart(ma);
                }

                Session["CART"] = cart;

                TempData["ThongBao"] =
                    "Đã xóa các sản phẩm được chọn.";
            }
            else
            {
                TempData["ThongBao"] =
                    "Bạn chưa chọn sản phẩm cần xóa.";
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}