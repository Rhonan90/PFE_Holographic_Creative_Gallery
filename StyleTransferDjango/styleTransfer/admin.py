from django.contrib import admin
from .models import ContentImagesModel, OutputImagesModel, StyleImagesModel

@admin.register(ContentImagesModel)
class ContentImagesAdmin(admin.ModelAdmin):
    list_display = ('id', 'img',)

@admin.register(OutputImagesModel)
class OutputImagesAdmin(admin.ModelAdmin):
    list_display = ('id', 'img',)

@admin.register(StyleImagesModel)
class StyleImagesAdmin(admin.ModelAdmin):
    list_display = ('name', 'img',)