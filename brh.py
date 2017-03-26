import os
import csv
from PIL import Image
from scipy import misc
import numpy as np

def average(pixel):
    return (pixel[0] + pixel[1] + pixel[2]) / 3

PATH1 = "C:\\Users\\Ishaan 2\\Pictures\\Camera Roll\\BRH1"
NEW_PATH1 = "C:\\Users\\Ishaan 2\\Pictures\\Camera Roll\\BRH2"
NEW_PATH2 = "C:\\Users\\Ishaan 2\\Pictures\\Camera Roll\\BRH3"
NEW_PATH3 = "C:\\Users\\Ishaan 2\\Pictures\\Camera Roll\\BRH4"

size = (128, 128)

# for root, dirs, files in os.walk(PATH1, topdown=False):
#     for name in files:
#         with open(root + "\\" + name, 'r+b') as f:
#             with Image.open(f) as img:
#                 # img = resizeimage.resize_crop(img, [350, 350])
#                 img = img.resize(size)
#                 img.save(NEW_PATH1 + "\\" + name, img.format)

for root, dirs, files in os.walk(NEW_PATH1, topdown=False):
    print(NEW_PATH1)
    for name in files:
        print(name)
        image = misc.imread(NEW_PATH1 + "\\" + name)
        grey = np.zeros((image.shape[0], image.shape[1]))  # init 2D numpy array
        # get row number
        for rownum in range(len(image)):
            for colnum in range(len(image[rownum])):
                grey[rownum][colnum] = average(image[rownum][colnum])

        #print("Before saving")
        misc.imsave(NEW_PATH2 + "\\" + name, grey)


with open(NEW_PATH3 + "\\" + "outfile.csv", "w", newline="") as f:
    fwriter = csv.writer(f, delimiter=",")
    for root, dirs, files in os.walk(NEW_PATH2, topdown=False):
        for name in files:
            slabel = name[4:6]
            if slabel == "AF":
                label = 0
            elif slabel == "AN":
                label = 1
            elif slabel == "DI":
                label = 2
            elif slabel == "HA":
                label = 3
            elif slabel == "NE":
                label = 4
            elif slabel == "SA":
                label = 5
            elif slabel == "SU":
                label = 6

            #If this script is used for test set generation use a label which is not a valid class example 8
            #label = 8

            im = Image.open(NEW_PATH1 + "\\" + name, 'r')
            pix_val = list(im.getdata())
            #print(pix_val)
            #pix_val_flat = [x for sets in pix_val for x in sets]
            pix_val_flat = []
            for sets in pix_val:
                if type(sets) == int:
                    pix_val_flat.append(sets)
                else:
                    pix_val_flat.append(int(average(sets)))
            #pix_vals = ",".join([str(x) for x in pix_val_flat])
            pix_vals = [str(label)]
            pix_vals.extend([str(x) for x in pix_val_flat])

            if name == "AF01AFS.JPG":
                print(pix_val)
                print(len(pix_val_flat))
                print(len(pix_vals))
            fwriter.writerow(pix_vals)
            #f.write(str(label) + "," + pix_vals)
