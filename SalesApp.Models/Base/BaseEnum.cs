using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoEStores.Core.Base
{
    public enum BaseEnum
    {
        Pending,    // Đang chờ duyệt / xử lý
        Active,     // Đang hoạt động
        Deleted,    // Đã bị xóa mềm
        Banned,     // Bị cấm (chỉ áp dụng cho user/store)
        Success,    // Giao thành công
        Failed,     // Thất bại
        Cancelled   // Đã huỷ
    }
}
