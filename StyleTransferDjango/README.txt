/!\ Django doit avoir une version strictement inférieure à 4 (développement sous Django 3.2.11)

Pour lancer le serveur :
1. Aller dans le dossier testServerDjango avec un invité de commandes
2. Ecrire "python manage.py runserver"
3. Dans un navigateur, se rendre à l'URL http://localhost:8000/style_transfer/
4. Vous devez vous retrouver devant l'interface vous permettant de tester le transfert de style

Pour supprimer des images :
1. Aller dans les dossiers média/content_images et media/output_images et supprimer les images
2. Se rendre à l'URL http://localhost:8000/admin/ dans un navigateur
3. Se connecter avec le compte admin (Username: admin / Password: admin)
4. Dans l'onglet STYLETRANSFER, supprimer les "Content images models" et les "Style images model"