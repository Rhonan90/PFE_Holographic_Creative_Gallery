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

class CameraCaptureTestServer(http.server.SimpleHTTPRequestHandler):

    def do_PUT(self):
          
        filename = self.path[1:]
        length = int(self.headers['Content-Length'])
        
        data = self.rfile.read(length)
        path= f'{os.getcwd()}/files/{filename}'
        print("PUT "+path)
        with open(path, 'wb') as file:
                file.write(data)
                subprocess.run(
        [sys.executable, "neural_style.py", "eval", \
                    "--content-image", "files/" + filename, \
                    "--model", "saved_models/mosaic.pth", \
                    "--output-image", "files/output_image/" + filename, \
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
        filename = self.path[1:]
        path= f'{os.getcwd()}/files/output_image/{filename}'
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
    server_address = ('', 8000)
    httpd = http.server.HTTPServer(server_address, CameraCaptureTestServer)
    httpd.serve_forever()
