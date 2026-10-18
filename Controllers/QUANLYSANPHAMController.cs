using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using QUANLYBANHANG_MVC.Models;

namespace QUANLYBANHANG_MVC.Controllers
{
    public class QUANLYSANPHAMController : Controller
    {
        private readonly dbQUANLYBANHANGEntities db =
            new dbQUANLYBANHANGEntities();

        private const int MaxImageBytes = 2 * 1024 * 1024;

        // GET: QUANLYSANPHAM/Index
        public ActionResult Index()
        {
            var sanPham = db.tbSANPHAMs
                .AsNoTracking()
                .Include(x => x.tbDANHMUC)
                .OrderByDescending(x => x.MASANPHAM)
                .ToList();

            return View(sanPham);
        }

        // GET: QUANLYSANPHAM/Create
        public ActionResult Create()
        {
            NapDanhMuc();

            return View(new SanPhamCreateViewModel());
        }

        // POST: QUANLYSANPHAM/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SanPhamCreateViewModel model)
        {
            if (model.DONGIA.HasValue &&
                decimal.Truncate(model.DONGIA.Value) != model.DONGIA.Value)
            {
                ModelState.AddModelError(
                    "DONGIA",
                    "Database hiện dùng numeric(18,0), đơn giá không được có phần lẻ.");
            }

            if (model.SOLUONG.HasValue &&
                decimal.Truncate(model.SOLUONG.Value) != model.SOLUONG.Value)
            {
                ModelState.AddModelError(
                    "SOLUONG",
                    "Số lượng phải là số nguyên.");
            }

            if (model.MADANHMUC.HasValue)
            {
                bool danhMucTonTai = db.tbDANHMUCs.Any(
                    x => x.MADANHMUC == model.MADANHMUC.Value);

                if (!danhMucTonTai)
                {
                    ModelState.AddModelError(
                        "MADANHMUC",
                        "Danh mục không tồn tại.");
                }
            }

            KiemTraThongTinFile(model);

            if (!ModelState.IsValid)
            {
                NapDanhMuc(model.MADANHMUC);
                return View(model);
            }

            string tenAnh = Guid.NewGuid().ToString("N") + ".png";
            string thuMucAnh = Server.MapPath("~/images");
            string duongDanAnh = Path.Combine(thuMucAnh, tenAnh);

            try
            {
                // Đọc nội dung thực tế, không chỉ kiểm tra tên file.
                using (var image = Image.FromStream(
                    model.AnhSanPham.InputStream,
                    true,
                    true))
                {
                    bool dungDinhDang =
                        image.RawFormat.Guid == ImageFormat.Jpeg.Guid ||
                        image.RawFormat.Guid == ImageFormat.Png.Guid;

                    if (!dungDinhDang)
                    {
                        ModelState.AddModelError(
                            "AnhSanPham",
                            "Nội dung file phải là ảnh JPEG hoặc PNG.");
                    }
                    else if (image.Width > 4096 || image.Height > 4096)
                    {
                        ModelState.AddModelError(
                            "AnhSanPham",
                            "Ảnh không được vượt quá 4096 x 4096 pixel.");
                    }
                    else
                    {
                        Directory.CreateDirectory(thuMucAnh);

                        // Mã hóa lại thành PNG, không lưu nguyên file upload.
                        using (var bitmap = new Bitmap(image))
                        {
                            bitmap.Save(duongDanAnh, ImageFormat.Png);
                        }
                    }
                }
            }
            catch (ArgumentException)
            {
                ModelState.AddModelError(
                    "AnhSanPham",
                    "File ảnh không hợp lệ hoặc đã bị hỏng.");
            }
            catch (OutOfMemoryException)
            {
                ModelState.AddModelError(
                    "AnhSanPham",
                    "Không thể đọc ảnh. Hãy chọn ảnh nhỏ hơn hoặc ảnh khác.");
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());

                ModelState.AddModelError(
                    "",
                    "Không thể lưu ảnh. Hãy kiểm tra quyền ghi thư mục images.");
            }

            if (!ModelState.IsValid)
            {
                XoaAnhNeuCo(duongDanAnh);
                NapDanhMuc(model.MADANHMUC);
                return View(model);
            }

            var sanPham = new tbSANPHAM
            {
                TENSANPHAM = model.TENSANPHAM.Trim(),
                DONGIA = model.DONGIA,
                SOLUONG = model.SOLUONG,
                MADANHMUC = model.MADANHMUC,
                MOTA = model.MOTA,
                HINHANH = tenAnh
            };

            try
            {
                db.tbSANPHAMs.Add(sanPham);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());

                // Tránh để lại ảnh khi bản ghi không lưu được.
                XoaAnhNeuCo(duongDanAnh);

                ModelState.AddModelError(
                    "",
                    "Không thể lưu sản phẩm. Vui lòng kiểm tra kết nối và dữ liệu.");

                NapDanhMuc(model.MADANHMUC);
                return View(model);
            }

            TempData["ThongBao"] = "Thêm sản phẩm thành công.";

            return RedirectToAction("Index");
        }
        // GET: QUANLYSANPHAM/Details/5
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var sanPham = db.tbSANPHAMs
                .AsNoTracking()
                .Include(x => x.tbDANHMUC)
                .SingleOrDefault(x => x.MASANPHAM == id.Value);

            if (sanPham == null)
            {
                return HttpNotFound();
            }

            return View(sanPham);
        }
        // GET: QUANLYSANPHAM/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var sanPham = db.tbSANPHAMs.Find(id.Value);

            if (sanPham == null)
            {
                return HttpNotFound();
            }

            var model = new SanPhamEditViewModel
            {
                MASANPHAM = sanPham.MASANPHAM,
                TENSANPHAM = sanPham.TENSANPHAM,
                DONGIA = sanPham.DONGIA,
                SOLUONG = sanPham.SOLUONG,
                MADANHMUC = sanPham.MADANHMUC,
                MOTA = sanPham.MOTA,
                HinhAnhHienTai = sanPham.HINHANH
            };

            NapDanhMuc(model.MADANHMUC);

            return View(model);
        }

        // POST: QUANLYSANPHAM/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, SanPhamEditViewModel model)
        {
            if (!id.HasValue || id.Value != model.MASANPHAM)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var sanPham = db.tbSANPHAMs.Find(id.Value);

            if (sanPham == null)
            {
                return HttpNotFound();
            }

            // Không tin tên ảnh do client gửi lên.
            model.HinhAnhHienTai = sanPham.HINHANH;

            if (model.DONGIA.HasValue &&
                decimal.Truncate(model.DONGIA.Value) != model.DONGIA.Value)
            {
                ModelState.AddModelError(
                    "DONGIA",
                    "Đơn giá không được có phần lẻ vì database dùng numeric(18,0).");
            }

            if (model.SOLUONG.HasValue &&
                decimal.Truncate(model.SOLUONG.Value) != model.SOLUONG.Value)
            {
                ModelState.AddModelError(
                    "SOLUONG",
                    "Số lượng phải là số nguyên.");
            }

            if (model.MADANHMUC.HasValue)
            {
                bool danhMucTonTai = db.tbDANHMUCs.Any(
                    x => x.MADANHMUC == model.MADANHMUC.Value);

                if (!danhMucTonTai)
                {
                    ModelState.AddModelError(
                        "MADANHMUC",
                        "Danh mục không tồn tại.");
                }
            }

            if (!ModelState.IsValid)
            {
                NapDanhMuc(model.MADANHMUC);
                return View(model);
            }

            // Không chọn ảnh mới thì biến này vẫn null.
            string tenAnhMoi = LuuAnhMoiKhiSua(model.AnhSanPham);

            if (!ModelState.IsValid)
            {
                NapDanhMuc(model.MADANHMUC);
                return View(model);
            }

            // Chỉ cập nhật các trường được phép sửa.
            sanPham.TENSANPHAM = model.TENSANPHAM.Trim();
            sanPham.DONGIA = model.DONGIA;
            sanPham.SOLUONG = model.SOLUONG;
            sanPham.MADANHMUC = model.MADANHMUC;
            sanPham.MOTA = model.MOTA;

            if (tenAnhMoi != null)
            {
                sanPham.HINHANH = tenAnhMoi;
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());

                // Nếu cập nhật database thất bại thì dọn ảnh vừa tạo.
                if (tenAnhMoi != null)
                {
                    XoaAnhNeuCo(
                        Path.Combine(Server.MapPath("~/images"), tenAnhMoi));
                }

                ModelState.AddModelError(
                    "",
                    "Không thể cập nhật sản phẩm. Vui lòng thử lại.");

                NapDanhMuc(model.MADANHMUC);
                return View(model);
            }

            TempData["ThongBao"] = "Cập nhật sản phẩm thành công.";

            return RedirectToAction("Index");
        }
        private string LuuAnhMoiKhiSua(HttpPostedFileBase file)
        {
            // Không chọn ảnh mới: giữ ảnh hiện tại.
            if (file == null)
            {
                return null;
            }

            if (file.ContentLength <= 0)
            {
                ModelState.AddModelError(
                    "AnhSanPham",
                    "File ảnh không được rỗng.");

                return null;
            }

            if (file.ContentLength > 2 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    "AnhSanPham",
                    "Dung lượng ảnh tối đa là 2 MB.");

                return null;
            }

            string extension = Path.GetExtension(file.FileName);

            bool dungDuoiFile =
                string.Equals(
                    extension, ".jpg", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    extension, ".jpeg", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    extension, ".png", StringComparison.OrdinalIgnoreCase);

            if (!dungDuoiFile)
            {
                ModelState.AddModelError(
                    "AnhSanPham",
                    "Chỉ chấp nhận file .jpg, .jpeg hoặc .png.");

                return null;
            }

            string tenAnhMoi = Guid.NewGuid().ToString("N") + ".png";
            string thuMucAnh = Server.MapPath("~/images");
            string duongDanAnh = Path.Combine(thuMucAnh, tenAnhMoi);

            try
            {
                using (var image = Image.FromStream(
                    file.InputStream,
                    true,
                    true))
                {
                    bool dungNoiDung =
                        image.RawFormat.Guid == ImageFormat.Jpeg.Guid ||
                        image.RawFormat.Guid == ImageFormat.Png.Guid;

                    if (!dungNoiDung)
                    {
                        ModelState.AddModelError(
                            "AnhSanPham",
                            "Nội dung file phải là ảnh JPEG hoặc PNG.");

                        return null;
                    }

                    if (image.Width > 4096 || image.Height > 4096)
                    {
                        ModelState.AddModelError(
                            "AnhSanPham",
                            "Ảnh không được vượt quá 4096 x 4096 pixel.");

                        return null;
                    }

                    Directory.CreateDirectory(thuMucAnh);

                    using (var bitmap = new Bitmap(image))
                    {
                        bitmap.Save(duongDanAnh, ImageFormat.Png);
                    }
                }

                return tenAnhMoi;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());

                XoaAnhNeuCo(duongDanAnh);

                ModelState.AddModelError(
                    "AnhSanPham",
                    "Không thể đọc hoặc lưu ảnh. Hãy chọn ảnh hợp lệ và kiểm tra quyền ghi.");

                return null;
            }
        }
        // GET: QUANLYSANPHAM/Delete/5
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var sanPham = db.tbSANPHAMs
                .AsNoTracking()
                .Include(x => x.tbDANHMUC)
                .SingleOrDefault(x => x.MASANPHAM == id.Value);

            if (sanPham == null)
            {
                return HttpNotFound();
            }

            ViewBag.DaCoTrongHoaDon = db.tbCHITIETHOADONs.Any(
                x => x.MASANPHAM == id.Value);

            return View(sanPham);
        }

        // POST: QUANLYSANPHAM/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var sanPham = db.tbSANPHAMs
                .Include(x => x.tbDANHMUC)
                .SingleOrDefault(x => x.MASANPHAM == id.Value);

            if (sanPham == null)
            {
                return HttpNotFound();
            }

            bool daCoTrongHoaDon = db.tbCHITIETHOADONs.Any(
                x => x.MASANPHAM == id.Value);

            ViewBag.DaCoTrongHoaDon = daCoTrongHoaDon;

            if (daCoTrongHoaDon)
            {
                ModelState.AddModelError(
                    "",
                    "Không thể xóa sản phẩm đã được sử dụng trong chi tiết hóa đơn.");

                return View("Delete", sanPham);
            }

            try
            {
                db.tbSANPHAMs.Remove(sanPham);
                db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                Trace.TraceError(ex.ToString());

                ModelState.AddModelError(
                    "",
                    "Không thể xóa. Sản phẩm có thể đang được dữ liệu khác tham chiếu.");

                return View("Delete", sanPham);
            }

            // Không tự xóa file ảnh để tránh ảnh dùng chung bị mất.
            TempData["ThongBao"] = "Xóa sản phẩm thành công.";

            return RedirectToAction("Index");
        }

        private void NapDanhMuc(int? selectedId = null)
        {
            var danhMuc = db.tbDANHMUCs
                .AsNoTracking()
                .OrderBy(x => x.TENDANHMUC)
                .ToList();

            ViewBag.DanhMucOptions = new SelectList(
                danhMuc,
                "MADANHMUC",
                "TENDANHMUC",
                selectedId);
        }

        private void KiemTraThongTinFile(
            SanPhamCreateViewModel model)
        {
            var file = model.AnhSanPham;

            if (file == null)
            {
                // Required trong ViewModel đã xử lý trường hợp này.
                return;
            }

            if (file.ContentLength <= 0)
            {
                ModelState.AddModelError(
                    "AnhSanPham",
                    "File ảnh không được rỗng.");

                return;
            }

            if (file.ContentLength > MaxImageBytes)
            {
                ModelState.AddModelError(
                    "AnhSanPham",
                    "Dung lượng ảnh tối đa là 2 MB.");
            }

            string extension = Path.GetExtension(file.FileName);

            bool dungDuoiFile =
                string.Equals(
                    extension, ".jpg", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    extension, ".jpeg", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    extension, ".png", StringComparison.OrdinalIgnoreCase);

            if (!dungDuoiFile)
            {
                ModelState.AddModelError(
                    "AnhSanPham",
                    "Chỉ chấp nhận file .jpg, .jpeg hoặc .png.");
            }
        }

        private static void XoaAnhNeuCo(string path)
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
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