using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.Enums
{
    public enum QuestionType
    {
        MultipleChoice = 1, // Trắc nghiệm nhiều lựa chọn
        OnlyChoice = 2, 
        TrueFalse = 3,      // Đúng/Sai
        Essay = 4           // Tự luận
    }
}
