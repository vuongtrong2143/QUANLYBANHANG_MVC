using System.ComponentModel.DataAnnotations;
using System.Web;

namespace QUANLYBANHANG_MVC.Models
{
    public class SanPhamCreateViewModel
    {
        [Display(Name = "Tên sản phẩm")]
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [StringLength(
            200,
            ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự.")]
        public string TENSANPHAM { get; set; }

        [Display(Name = "Đơn giá")]
        [Required(ErrorMessage = "Vui lòng nhập đơn giá.")]
        [Range(
            typeof(decimal),
            "0",
            "999999999999999999",
            ErrorMessage = "Đơn giá phải từ 0 đến 999999999999999999.")]
        public decimal? DONGIA { get; set; }

        [Display(Name = "Số lượng")]
        [Required(ErrorMessage = "Vui lòng nhập số lượng.")]
        [Range(
            typeof(decimal),
            "0",
            "999999999999999999",
            ErrorMessage = "Số lượng phải từ 0 đến 999999999999999999.")]
        public decimal? SOLUONG { get; set; }

        [Display(Name = "Danh mục")]
        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        public int? MADANHMUC { get; set; }

        [Display(Name = "Mô tả")]
        [DataType(DataType.MultilineText)]
        public string MOTA { get; set; }

        [Display(Name = "Ảnh sản phẩm")]
        [Required(ErrorMessage = "Vui lòng chọn ảnh sản phẩm.")]
        public HttpPostedFileBase AnhSanPham { get; set; }
    }
}