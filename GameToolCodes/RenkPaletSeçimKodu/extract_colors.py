import os
import json
from PIL import Image
import numpy as np
from sklearn.cluster import KMeans
from collections import Counter

def get_dominant_colors(image_path, k=5):
    """
    Extracts k dominant colors from an image using K-Means clustering.
    """
    try:
        image = Image.open(image_path)
        image = image.convert('RGB')
        # Resize image to speed up processing
        image = image.resize((100, 100))
        image_array = np.array(image)
        image_array = image_array.reshape((image_array.shape[0] * image_array.shape[1], 3))

        clt = KMeans(n_clusters=k, n_init=10)
        clt.fit(image_array)

        # Get the colors
        colors = clt.cluster_centers_
        
        # Convert to hex
        hex_colors = []
        for color in colors:
            hex_colors.append('#{:02x}{:02x}{:02x}'.format(int(color[0]), int(color[1]), int(color[2])))
            
        return hex_colors
    except Exception as e:
        print(f"Error processing {image_path}: {e}")
        return []

def main():
    folder_path = r"c:/Users/democh/Downloads/renkPaletleri/avatarWorld"
    output_file = os.path.join(folder_path, "palettes.json")
    
    results = {}
    
    files = [f for f in os.listdir(folder_path) if f.lower().endswith(('.jpg', '.jpeg', '.png'))]
    
    print(f"Found {len(files)} images.")
    
    for file in files:
        print(f"Processing {file}...")
        file_path = os.path.join(folder_path, file)
        colors = get_dominant_colors(file_path, k=5)
        results[file] = colors
        print(f"  Colors: {colors}")

    with open(output_file, 'w') as f:
        json.dump(results, f, indent=4)
        
    print(f"Done! Results saved to {output_file}")

if __name__ == "__main__":
    main()
