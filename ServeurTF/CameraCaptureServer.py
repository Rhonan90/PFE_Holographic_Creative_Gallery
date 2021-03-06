import http.server
import http.server
import io
import os
import sys
from functools import partial
from http import HTTPStatus
import neural_style
from argparse import Namespace
import subprocess
from PIL import Image

class CameraCaptureTestServer(http.server.SimpleHTTPRequestHandler):

    def do_PUT(self):
        
        #filename_prov contains "style choice" + "$" + "file name"
        filename_prov = self.path[1:]
        #Separate style choice and file name
        res = filename_prov.split("$")
        stylechoice = res[0]
        print ("STYLECHOICE : " + stylechoice)
        filename = res[1]
        print("PHOTO : " + filename)
        length = int(self.headers['Content-Length'])
        data = self.rfile.read(length)
        path= f'{os.getcwd()}/files/{filename}'
        print("PUT "+path)
        #Write the data in the right file
        with open(path, 'wb') as file:
                file.write(data)
        #Open the image corresponding to the file
        imagepng = Image.open(path)
        #Compression to make the image less heavy -> style transfer is faster
        imagejpg = imagepng.convert('RGB')
        w, h = imagejpg.size
        imageComp=imagejpg.resize((int(w/4),int(h/4)))
        imageComp.save("files/content.jpg")
        #Following line launch the style transfer of the current image with the style we chose
        subprocess.run([sys.executable, "neural_style.py", "eval", \
                    "--content-image", "files/content.jpg", \
                    "--model", "saved_models/" + stylechoice + ".pth", \
                    "--output-image", "files/output_image/result.jpg", \
                    "--cuda", "0"])
        print("style ok")
        enc = sys.getfilesystemencoding()
        output = filename.encode(enc, 'surrogateescape')
        self.wfile.write(output)
        self.send_response(HTTPStatus.OK)
        self.send_header('Content-Type', 'text/html')
        self.send_header('Content-Length', str(len(output)))
        self.end_headers()

    def do_GET(self):
        #Select the stylized image 
        filename = self.path[1:]
        path= f'{os.getcwd()}/files/output_image/result.jpg'
        print("GET "+path)

        if not os.path.exists(path):
            enc = sys.getfilesystemencoding()
            output = "not found".encode(enc, 'surrogateescape')
            f = io.BytesIO()
            f.write(output)
            f.seek(0)
            self.send_response(HTTPStatus.NOT_FOUND)
            self.send_header('Content-Type', 'text/html')
            self.send_header('Content-Length', str(len(output)))
            self.end_headers()

            print(output)
            
        else:
            f = open(path, 'rb')
            
            self.send_response(HTTPStatus.OK)
            self.send_header('Content-Type', 'image/png')
            #self.send_header('Content-Length', str(len(data)))
            self.end_headers()
            self.wfile.write(f.read())
            f.close()

if __name__ == '__main__':
    server_address = ('', 33900)
    httpd = http.server.HTTPServer(server_address, CameraCaptureTestServer)
    httpd.serve_forever()
