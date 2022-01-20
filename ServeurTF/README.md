# Style transfer server
This repository contains a server using python and pytorch in order to do style transfer. The server is inspired from [this repository](https://github.com/pytorch/examples/tree/master/fast_neural_style)

## Requirements
To run the program, you need to have Python installed, as [Pytorch](http://pytorch.org/) and [Scipy](https://www.scipy.org).

## Usage - RUN THE SERVER : 

* Clone the git repository
* Make sure your device is connected to the same network as the Hololens. 
* From the command line, go to the repository PFE_Holographic_Creative_Gallery/ServeurTF
* Run the following line : 
```bash
python CameraCaptureServer.py runserver
```



## BONUS : How to train model

Here is the script you'll have to run if you want to train a model : 
```bash
python neural_style/neural_style.py train --dataset </path/to/train-dataset> --style-image </path/to/style/image> --save-model-dir </path/to/save-model/folder> --epochs 2 --cuda 1
```

There are several command line arguments, the important ones are listed below
* `--dataset`: path to training dataset, the path should point to a folder containing another folder with all the training images. I used COCO 2014 Training images dataset [80K/13GB] [(download)](https://cocodataset.org/#download).
* `--style-image`: path to style-image.
* `--save-model-dir`: path to folder where trained model will be saved.
* `--cuda`: set it to 1 for running on GPU, 0 for CPU.

Refer to ``neural_style/neural_style.py`` for other command line arguments. For training new models you might have to tune the values of `--content-weight` and `--style-weight`. The mosaic style model shown above was trained with `--content-weight 1e5` and `--style-weight 1e10`. The remaining 3 models were also trained with similar order of weight parameters with slight variation in the `--style-weight` (`5e10` or `1e11`).
