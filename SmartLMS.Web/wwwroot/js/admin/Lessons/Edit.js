function handleVideoUpload(input) {
    const file = input.files[0];

    if (!file) {
        clearVideoPreview();
        return;
    }

    if (!file.type.startsWith('video/')) {
        Swal.fire({
            icon: 'error',
            title: 'Lỗi',
            text: 'Vui lòng chọn file video hợp lệ!'
        });
        input.value = '';
        return;
    }

    const maxSize = 500 * 1024 * 1024;
    if (file.size > maxSize) {
        Swal.fire({
            icon: 'error',
            title: 'File quá lớn',
            text: 'Kích thước video không được vượt quá 500MB!'
        });
        input.value = '';
        return;
    }

    showVideoLoading();

    const videoUrl = URL.createObjectURL(file);
    const video = document.createElement('video');

    video.preload = 'metadata';

    video.onloadedmetadata = function () {
        URL.revokeObjectURL(videoUrl);

        const durationInSeconds = Math.round(video.duration);

        $('#Duration').val(durationInSeconds);

        showVideoPreview(videoUrl, durationInSeconds, file);

        if (typeof notion !== 'undefined') {
            notion.success(`Video đã được tải lên. Thời lượng: ${formatDuration(durationInSeconds)}`, 3000);
        }
    };

    video.onerror = function () {
        URL.revokeObjectURL(videoUrl);
        Swal.fire({
            icon: 'error',
            title: 'Lỗi',
            text: 'Không thể đọc thông tin video. Vui lòng chọn file khác!'
        });
        input.value = '';
        clearVideoPreview();
    };

    video.src = videoUrl;
}

function showVideoLoading() {
    $('#videoPreview').html(`
        <div class="text-center py-3">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Đang xử lý...</span>
            </div>
            <p class="mt-2 mb-0 text-muted">Đang phân tích video...</p>
        </div>
    `);
}

function showVideoPreview(videoUrl, duration, file) {
    const fileSize = (file.size / (1024 * 1024)).toFixed(2); // MB
    const durationFormatted = formatDuration(duration);

    const previewHtml = `
        <div class="card mt-2">
            <div class="card-body">
                <div class="d-flex align-items-center">
                    <div class="me-3">
                        <i class="fas fa-video fa-3x text-primary"></i>
                    </div>
                    <div class="flex-grow-1">
                        <h6 class="mb-1">${file.name}</h6>
                        <small class="text-muted">
                            <i class="fas fa-clock me-1"></i>${durationFormatted}
                            <span class="mx-2">•</span>
                            <i class="fas fa-file me-1"></i>${fileSize} MB
                        </small>
                    </div>
                    <div>
                        <button type="button" class="btn btn-sm btn-danger" onclick="clearVideoUpload()">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;

    $('#videoPreview').html(previewHtml);
}

function clearVideoPreview() {
    $('#videoPreview').empty();
}

function clearVideoUpload() {
    $('input[name="video"]').val('');
    clearVideoPreview();
    $('#Duration').val('');

    if (typeof notion !== 'undefined') {
        notion.info('Đã xóa video', 2000);
    }
}

function formatDuration(seconds) {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;

    if (hours > 0) {
        return `${pad(hours)}:${pad(minutes)}:${pad(secs)}`;
    }
    return `${pad(minutes)}:${pad(secs)}`;
}


function pad(num) {
    return num.toString().padStart(2, '0');
}
$(document).on('submit', '#editLessonForm', function (e) {
    e.preventDefault();

    var formData = new FormData(this);

    $.ajax({
        url: $(this).attr('action'),
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.success) {
                $('#modalContainer').modal('hide');
                notion.success("Sửa bài học thành công!", 4000);
                showpopup(
                    'Quản lý bài học',
                    '/Admin/Lessons?courseId=' + response.courseId,
                );
            }
        },
        error: function (xhr) {
            if (xhr.status === 400) {
                $('#modalContainer .modal-body').html(xhr.responseText);
            } else {
                toastr.error('Có lỗi xảy ra!');
            }
        }
    });
});