from django.urls import path
from . import views

from django.urls import path
from django.conf import settings
from django.conf.urls.static import static

urlpatterns = [
    path('', views.index, name='index'),
    path('images', views.display_images, name = 'images'),
    path('content_image/<int:pk>', views.ContentImagesDetailView.as_view(), name='content-image-detail'),
    path('content_image/<int:pk>/delete/', views.ContentImagesDelete.as_view(), name='content-image-delete'),
]

if settings.DEBUG:
        urlpatterns += static(settings.MEDIA_URL,
                              document_root=settings.MEDIA_ROOT)