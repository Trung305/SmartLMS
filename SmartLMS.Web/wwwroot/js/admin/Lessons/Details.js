$(document).on('submit', '#hideOrPublishLessonForm', function (e) {
    e.preventDefault();
    const lessonId = $(this).closest('form').data('lesson-id'); 
    const courseId = $(this).closest('form').data('course-id');
    const button = e.originalEvent.submitter; 
    const type = $(button).data('type');
    fetch(`/admin/lessons/TogglePublish/${lessonId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    })
        .then(res => res.json())
        .then(data => {
            if (!data.success) {
                showError(data.message || 'Có lỗi xảy ra!');
                return;
            }

            type == 1 ? showSuccess("Xuất bản thành công!") : showSuccess("Ản bài học thành công!");
            showpopup(
                'Quản lý bài học',
                '/Admin/Lessons?courseId=' + courseId,
            );
        })
        .catch(err => {
            console.log('Delete error:', err);
            type == 1 ? showError("Lỗi hệ thống! Không thể xuất bản bài học.") : showError("Lỗi hệ thống! Không thể ẩn bài học.");
            
        });
    
});
$(document).off('click', '#deleteLesson')
    .on('click', '#deleteLesson', function () {
        const lessonId = $(this).data('lesson-id');
        const courseId = $(this).data('course-id');
        Swal.fire({
            title: 'Cảnh báo',
            text: "Bạn có chắc muốn xóa bài học này không?",
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: '<i class="fas fa-trash"></i> Xóa',
            cancelButtonText: '<i class="fas fa-times"></i> Hủy'
        }).then((result) => {
            if (!result.isConfirmed) return;

            fetch(`/admin/lessons/Delete/${lessonId}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
                .then(res => res.json())
                .then(data => {
                    if (!data.success) {
                        showError(data.message || "Không thể xóa bài học!");
                        return;
                    }

                    showSuccess("Xóa thành công!");
                    showpopup(
                        'Quản lý bài học',
                        '/Admin/Lessons?courseId=' + courseId,
                    );
                })
                .catch(err => {
                    console.error('Delete error:', err);
                    showError("Lỗi hệ thống! Không thể xóa bài học.");
                });
        });
    });
function showSuccess(message) {
    if (typeof notion !== 'undefined') {
        notion.success(message, 4000);
    } else {
        alert(message);
    }
}

function showError(message) {
    if (typeof notion !== 'undefined') {
        notion.error(message, 4000);
    } else {
        alert(message);
    }
}