$(document).on('popup:loaded', function () {
    if ($('#lessonsList').length > 0) {
        window.initLessonsIndex();
    }
});

$(document).on('hidden.bs.modal', '#popupModal', function () {
    $('#lessonsList').removeData('lessons-initialized');
});

window.initLessonsIndex = function () {

    const courseId = window.lessonsData?.courseId || $('#lessonsList').data('course-id');

    if (!courseId) {
        console.error('courseId not found!');
        showError('Không tìm thấy khóa học');
        return;
    }

    let sortable = null;
    let lessonsCache = [];
    let quizzesCache = [];
    loadLessons();

    function loadLessons() {
        $('#loadingCard').show();
        $('#courseInfoCard, #lessonsCard').hide();

        fetch(`/Admin/Lessons/GetLessonsByCourse?courseId=${courseId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to load lessons');
                }
                return response.json();
            })
            .then(result => {
                if (!result.success) {
                    throw new Error(result.message || 'Không thể tải danh sách bài học');
                }

                lessonsCache = result.data.lessons;
                quizzesCache = result.data.quizzes;
                renderCourseInfo(result.data.course, result.data.lessons);
                renderLessonsList(result.data.lessons);
                renderQuizzesList(result.data.quizzes, result.data.lessons);
                $('#loadingCard').hide();
                $('#courseInfoCard, #lessonsCard').show();
            })
            .catch(error => {
                console.error('Error loading lessons:', error);
                showError(error.message || 'Có lỗi xảy ra khi tải danh sách bài học');
                $('#loadingCard').hide();
            });
    }

    function renderCourseInfo(course, lessons) {
        $('#courseTitleHeader').text(course.title);
        $('#instructorName').text(course.instructor?.fullName || '-');
        $('#categoryName').text(course.category?.name || '-');
        $('#totalLessons').text(lessons.length);

        const totalSeconds = lessons.reduce((sum, l) => sum + l.duration, 0);
        const hours = Math.floor(totalSeconds / 3600);
        const minutes = Math.floor((totalSeconds % 3600) / 60);
        const seconds = totalSeconds % 60;
        $('#totalDuration').text(`${pad(hours)}:${pad(minutes)}:${pad(seconds)}`);

        const thumbnailHtml = course.thumbnail
            ? `<img src="${course.thumbnail}" alt="${course.title}" class="img-fluid rounded">`
            : `<div class="bg-light rounded d-flex align-items-center justify-content-center" style="height: 150px;">
                   <i class="fas fa-image fa-2x text-muted"></i>
               </div>`;
        $('#courseThumbnailContainer').html(thumbnailHtml);
    }

    function renderLessonsList(lessons) {
        const $list = $('#lessonsList');
        const $emptyState = $('#emptyState');
        const $enableReorder = $('#enableReorder');

        if (!lessons || lessons.length === 0) {
            $list.empty().hide();
            $emptyState.show();
            $enableReorder.hide();
            $('#lessonCount').text('0');
            return;
        }

        $emptyState.hide();
        $list.show();
        $enableReorder.show();
        $('#lessonCount').text(lessons.length);

        // Chỉ hiện scroll khi có 3+ items
        if (lessons.length >= 3) {
            $list.addClass('scrollable-list');
        } else {
            $list.removeClass('scrollable-list');
        }

        const html = lessons.map(lesson => createLessonItem(lesson)).join('');
        $list.html(html);
    }

    function renderQuizzesList(quizzes, lessons) {
        const $list = $('#quizzesList');
        const $emptyState = $('#emptyQuizzesState');

        if (!quizzes || quizzes.length === 0) {
            $list.empty().hide();
            $emptyState.show();
            $('#quizCount').text('0');
            return;
        }

        $emptyState.hide();
        $list.show();
        $('#quizCount').text(quizzes.length);

        // Chỉ hiện scroll khi có 3+ items
        if (quizzes.length >= 3) {
            $list.addClass('scrollable-list');
        } else {
            $list.removeClass('scrollable-list');
        }

        const standaloneQuizzes = quizzes.filter(q => !q.lessonId);
        const linkedQuizzes = quizzes.filter(q => q.lessonId);

        let html = '';

        if (standaloneQuizzes.length > 0) {
            html += '<h6 class="text-muted mb-3"><i class="fas fa-star me-2"></i>Bài kiểm tra tổng hợp</h6>';
            standaloneQuizzes.forEach(quiz => {
                html += createQuizCard(quiz, lessons);
            });
        }

        if (linkedQuizzes.length > 0) {
            html += '<h6 class="text-muted mb-3 mt-4"><i class="fas fa-link me-2"></i>Bài kiểm tra theo bài học</h6>';
            linkedQuizzes.forEach(quiz => {
                html += createQuizCard(quiz, lessons);
            });
        }

        $list.html(html);
    }

    function createQuizCard(quiz, lessons) {
        const linkedLesson = quiz.lessonId ? lessons.find(l => l.id === quiz.lessonId) : null;
        const isStandalone = !quiz.lessonId;

        const timeInfo = quiz.isTimedQuiz
            ? `${quiz.timeLimit} phút`
            : 'Không giới hạn';

        let lessonConnectionText = '';
        if (linkedLesson) {
            lessonConnectionText = `Bài ${linkedLesson.orderIndex}: ${escapeHtml(linkedLesson.title)}`;
        } else if (isStandalone && lessons && lessons.length > 0) {
            const lessonNames = lessons.slice(0, 3).map(l => `Bài ${l.orderIndex}`).join(', ');
            lessonConnectionText = lessons.length > 3
                ? `${lessonNames} và ${lessons.length - 3} bài khác`
                : lessonNames;
        }

        return `
        <div class="list-group-item d-flex justify-content-between align-items-center" data-quiz-id="${quiz.id}">
            <div>
                <h6 class="mb-1">
                    ${isStandalone
                ? '<span class="badge bg-primary me-2"><i class="fas fa-star me-1"></i>Tổng hợp</span>'
                : '<span class="badge bg-success me-2"><i class="fas fa-link me-1"></i>Gắn bài học</span>'
            }
                    ${escapeHtml(quiz.title)}
                </h6>
                <small class="text-muted">
                    <i class="fas fa-question-circle me-1"></i>${quiz.totalQuestions || 0} câu hỏi • 
                    <i class="fas fa-star me-1"></i>${quiz.totalPoints || 0} điểm • 
                    <i class="fas fa-clock me-1"></i>Thời gian: ${timeInfo}
                </small>
                ${lessonConnectionText ? `
                    <div class="mt-1">
                        <small class="text-muted">
                            <i class="fas fa-link me-1"></i><strong>Liên kết:</strong> ${lessonConnectionText}
                        </small>
                    </div>
                ` : ''}
                <div class="mt-2">
                    ${!quiz.isActive ? '<span class="badge bg-danger me-2"><i class="fas fa-ban me-1"></i>Không hoạt động</span>' : ''}
                    ${quiz.isTimedQuiz ? '<span class="badge bg-warning text-dark me-2"><i class="fas fa-stopwatch me-1"></i>Có giới hạn TG</span>' : ''}
                    <span class="badge bg-info me-2">
                        <i class="fas fa-check-circle me-1"></i>Điểm đậu: ${quiz.passingScore}%
                    </span>
                    <span class="badge bg-secondary me-2">
                        <i class="fas fa-redo me-1"></i>Tối đa ${quiz.maxAttempts} lần
                    </span>
                </div>
            </div>
            <div class="d-flex align-items-center gap-2">
                <a onclick="showpopup('Chi tiết bài kiểm tra', '/Admin/Quizzes/Details/${quiz.id}'); return false;"
                   class="btn btn-sm btn-outline-info" title="Xem chi tiết">
                    <i class="fas fa-eye"></i>
                </a>
                <a onclick="showpopup('Quản lý câu hỏi', '/Admin/Questions/Index/${quiz.id}'); return false;"
                   class="btn btn-sm btn-outline-success" title="Quản lý câu hỏi">
                    <i class="fas fa-list"></i>
                </a>
                <a onclick="showpopup('Chỉnh sửa bài kiểm tra', '/Admin/Quizzes/Edit/${quiz.id}'); return false;"
                   class="btn btn-sm btn-outline-primary" title="Sửa">
                    <i class="fas fa-edit"></i>
                </a>
                <button type="button" class="btn btn-sm btn-outline-danger btn-delete-quiz"
                        data-quiz-id="${quiz.id}" title="Xóa">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
        </div>
    `;
    }

    function createLessonItem(lesson) {
        const duration = formatDuration(lesson.duration);
        const contentPreview = lesson.content
            ? (lesson.content.substring(0, 150) + (lesson.content.length > 150 ? '...' : ''))
            : 'Chưa có nội dung';

        return `
        <div class="list-group-item d-flex justify-content-between align-items-center" data-lesson-id="${lesson.id}">
            <div>
                <h6 class="mb-1">
                    <span class="badge bg-secondary me-2">Bài ${lesson.orderIndex}</span>
                    ${escapeHtml(lesson.title)}
                </h6>
                <small class="text-muted">
                    <i class="fas fa-clock me-1"></i>Thời lượng: ${duration}
                </small>
                <div class="mt-2">
                    ${lesson.isPreview ? '<span class="badge bg-success me-2"><i class="fas fa-eye me-1"></i>Xem trước</span>' : ''}
                    ${lesson.isPublished
                ? '<span class="badge bg-success me-2"><i class="fas fa-check me-1"></i>Đã xuất bản</span>'
                : '<span class="badge bg-warning text-dark me-2"><i class="fas fa-eye-slash me-1"></i>Nháp</span>'}
                    ${lesson.videoUrl ? '<span class="badge bg-primary me-2"><i class="fas fa-video me-1"></i>Có video</span>' : ''}
                </div>
            </div>

            <div class="d-flex align-items-center gap-2">
                <span class="drag-handle me-3" style="cursor: move; display: none; font-size: 1.2rem;">
                    <i class="fas fa-grip-vertical text-muted"></i>
                </span>
                <a onclick="showpopup('Chi tiết bài học', '/Admin/Lessons/Details/${lesson.id}'); return false;"
                   class="btn btn-sm btn-outline-info" title="Xem chi tiết">
                    <i class="fas fa-eye"></i>
                </a>
                <a onclick="showpopup('Chỉnh sửa bài học', '/Admin/Lessons/Edit/${lesson.id}'); return false;"
                   class="btn btn-sm btn-outline-primary" title="Sửa">
                    <i class="fas fa-edit"></i>
                </a>
                <button type="button" class="btn btn-sm btn-outline-danger btn-delete-lesson"
                        data-lesson-id="${lesson.id}" title="Xóa">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
        </div>
    `;
    }

    $(document).off('click', '.btn-delete-lesson').on('click', '.btn-delete-lesson', function () {
        const lessonId = $(this).data('lesson-id');

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
                    setTimeout(() => loadLessons(), 1000);
                })
                .catch(err => {
                    console.error('Delete error:', err);
                    showError("Lỗi hệ thống! Không thể xóa bài học.");
                });
        });
    });

    $(document).off('click', '.btn-delete-quiz').on('click', '.btn-delete-quiz', function () {
        const quizId = $(this).data('quiz-id');

        Swal.fire({
            title: 'Cảnh báo',
            text: "Bạn có chắc muốn xóa bài kiểm tra này không?",
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: '<i class="fas fa-trash"></i> Xóa',
            cancelButtonText: '<i class="fas fa-times"></i> Hủy'
        }).then((result) => {
            if (!result.isConfirmed) return;

            fetch(`/admin/Quizzes/Delete/${quizId}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
                .then(res => res.json())
                .then(data => {
                    if (!data.success) {
                        showError(data.message || "Không thể xóa bài kiểm tra!");
                        return;
                    }

                    showSuccess("Xóa bài kiểm tra thành công!");
                    setTimeout(() => loadLessons(), 1000);
                })
                .catch(err => {
                    console.error('Delete error:', err);
                    showError("Lỗi hệ thống! Không thể xóa kiểm tra.");
                });
        });
    });

    $('#enableReorder').off('click').on('click', function () {
        $(this).hide();
        $('.drag-handle').show();
        $('#reorderButtons').show();

        const el = document.getElementById('lessonsList');
        sortable = Sortable.create(el, {
            handle: '.drag-handle',
            animation: 150
        });
    });

    $('#cancelReorder').off('click').on('click', function () {
        renderLessonsList(lessonsCache);
        $('#enableReorder').show();
        $('.drag-handle').hide();
        $('#reorderButtons').hide();

        if (sortable) {
            sortable.destroy();
            sortable = null;
        }
    });

    $('#saveOrder').off('click').on('click', function () {
        const lessonIds = [];
        $('#lessonsList .list-group-item').each(function () {
            lessonIds.push($(this).data('lesson-id'));
        });

        if (lessonIds.length === 0) {
            showError('Không có bài học nào để sắp xếp');
            return;
        }

        fetch('/admin/lessons/reorder', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                courseId: courseId,
                lessonIds: lessonIds
            })
        })
            .then(res => res.json())
            .then(response => {
                if (response.success) {
                    showSuccess(response.message || 'Đã cập nhật thứ tự bài học');
                    $('#enableReorder').show();
                    $('.drag-handle').hide();
                    $('#reorderButtons').hide();
                    setTimeout(() => loadLessons(), 1000);
                } else {
                    showError(response.message || 'Có lỗi xảy ra');
                }
            })
            .catch(err => {
                console.error('Reorder error:', err);
                showError('Có lỗi xảy ra khi lưu thứ tự');
            });
    });

    function formatDuration(seconds) {
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return `${pad(mins)}:${pad(secs)}`;
    }

    function pad(num) {
        return num.toString().padStart(2, '0');
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

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
};