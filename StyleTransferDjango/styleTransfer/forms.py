
from django import forms
from .models import ContentImagesModel
from .choices import STYLE_IMAGE_CHOICES

class ContentImagesForm(forms.ModelForm):
    style_image = forms.ChoiceField(choices = STYLE_IMAGE_CHOICES)

    class Meta:
        model = ContentImagesModel
        fields = ['img']