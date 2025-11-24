import tkinter as tk
from tkinter import filedialog, messagebox
from PIL import Image, ImageTk
import json
import os
import colorsys

class ColorPickerApp:
    def __init__(self, root):
        self.root = root
        self.root.title("Unity Color Picker - Golden Style & Prompts")
        self.root.geometry("1400x900")

        self.current_image_path = None
        self.current_image = None
        self.tk_image = None
        self.json_path = os.path.join(os.path.dirname(__file__), "palettes.json")
        self.palettes = self.load_palettes()

        # UI Elements
        self.setup_ui()

    def setup_ui(self):
        # Top Control Panel
        control_frame = tk.Frame(self.root, pady=10)
        control_frame.pack(fill=tk.X)

        btn_open = tk.Button(control_frame, text="Open Image", command=self.open_image, font=("Arial", 12))
        btn_open.pack(side=tk.LEFT, padx=20)

        btn_update = tk.Button(control_frame, text="Load & Update JSON", command=self.load_and_update_json, font=("Arial", 12), bg="#e1f5fe")
        btn_update.pack(side=tk.LEFT, padx=20)

        self.lbl_status = tk.Label(control_frame, text="No image loaded", font=("Arial", 12))
        self.lbl_status.pack(side=tk.LEFT, padx=20)

        # Main Content Area
        content_frame = tk.Frame(self.root)
        content_frame.pack(fill=tk.BOTH, expand=True)

        # Image Canvas (Left)
        self.canvas_frame = tk.Frame(content_frame, bg="gray")
        self.canvas_frame.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        
        self.canvas = tk.Canvas(self.canvas_frame, bg="#333333", cursor="cross")
        self.canvas.pack(fill=tk.BOTH, expand=True)
        self.canvas.bind("<Button-1>", self.on_canvas_click)

        # Sidebar (Right) - Color List
        sidebar = tk.Frame(content_frame, width=450, bg="#f0f0f0")
        sidebar.pack(side=tk.RIGHT, fill=tk.Y)
        sidebar.pack_propagate(False)

        lbl_colors = tk.Label(sidebar, text="Picked Styles & Prompts", font=("Arial", 14, "bold"), bg="#f0f0f0")
        lbl_colors.pack(pady=10)

        self.listbox_frame = tk.Frame(sidebar)
        self.listbox_frame.pack(fill=tk.BOTH, expand=True, padx=10)
        
        self.canvas_list = tk.Canvas(self.listbox_frame, bg="#f0f0f0")
        self.scrollbar = tk.Scrollbar(self.listbox_frame, orient="vertical", command=self.canvas_list.yview)
        self.scrollable_frame = tk.Frame(self.canvas_list, bg="#f0f0f0")

        self.scrollable_frame.bind(
            "<Configure>",
            lambda e: self.canvas_list.configure(scrollregion=self.canvas_list.bbox("all"))
        )

        self.canvas_list.create_window((0, 0), window=self.scrollable_frame, anchor="nw")
        self.canvas_list.configure(yscrollcommand=self.scrollbar.set)

        self.canvas_list.pack(side="left", fill="both", expand=True)
        self.scrollbar.pack(side="right", fill="y")

        # Bind MouseWheel
        self.canvas_list.bind_all("<MouseWheel>", self._on_mousewheel)

        btn_clear = tk.Button(sidebar, text="Clear Colors for this Image", command=self.clear_colors, bg="#ffcccc")
        btn_clear.pack(pady=10, padx=10, fill=tk.X)

    def _on_mousewheel(self, event):
        self.canvas_list.yview_scroll(int(-1*(event.delta/120)), "units")

    def load_palettes(self):
        if os.path.exists(self.json_path):
            try:
                with open(self.json_path, 'r') as f:
                    return json.load(f)
            except:
                return {}
        return {}

    def save_palettes(self):
        with open(self.json_path, 'w') as f:
            json.dump(self.palettes, f, indent=4)

    def load_and_update_json(self):
        file_path = filedialog.askopenfilename(
            initialdir=os.path.dirname(__file__),
            title="Select JSON to Update",
            filetypes=[("JSON Files", "*.json")]
        )
        
        if not file_path:
            return

        try:
            with open(file_path, 'r') as f:
                data = json.load(f)
            
            updated_count = 0
            for filename, styles in data.items():
                new_styles = []
                for style in styles:
                    if 'base' in style:
                        # Regenerate using the base color
                        new_style = self.generate_style(style['base'])
                        new_styles.append(new_style)
                        updated_count += 1
                    else:
                        new_styles.append(style)
                data[filename] = new_styles
            
            # Save back
            with open(file_path, 'w') as f:
                json.dump(data, f, indent=4)
            
            # Reload into app
            self.json_path = file_path
            self.palettes = data
            self.refresh_color_list()
            
            messagebox.showinfo("Success", f"Updated {updated_count} palettes with new style/prompt logic!")
            
        except Exception as e:
            messagebox.showerror("Error", f"Failed to update JSON: {e}")

    def open_image(self):
        file_path = filedialog.askopenfilename(
            initialdir=os.path.dirname(__file__),
            title="Select Image",
            filetypes=[("Image Files", "*.jpg *.jpeg *.png")]
        )
        
        if file_path:
            self.load_image(file_path)

    def load_image(self, path):
        self.current_image_path = path
        filename = os.path.basename(path)
        self.lbl_status.config(text=f"Editing: {filename}")

        pil_img = Image.open(path)
        
        # Max dimensions
        max_w, max_h = 1000, 800
        img_w, img_h = pil_img.size
        
        scale = min(max_w/img_w, max_h/img_h, 1.0)
        new_w = int(img_w * scale)
        new_h = int(img_h * scale)
        
        self.current_image = pil_img.resize((new_w, new_h), Image.Resampling.LANCZOS)
        self.tk_image = ImageTk.PhotoImage(self.current_image)
        
        self.canvas.config(width=new_w, height=new_h)
        self.canvas.create_image(0, 0, anchor=tk.NW, image=self.tk_image)
        
        self.refresh_color_list()

    def on_canvas_click(self, event):
        if not self.current_image:
            return

        x, y = event.x, event.y
        
        if x < 0 or y < 0 or x >= self.current_image.width or y >= self.current_image.height:
            return

        r, g, b = self.current_image.getpixel((x, y))
        hex_color = '#{:02x}{:02x}{:02x}'.format(r, g, b)
        
        self.add_color_style(hex_color)

    def generate_style(self, base_hex):
        """
        Generates a style palette and a prompt.
        """
        r, g, b = int(base_hex[1:3], 16), int(base_hex[3:5], 16), int(base_hex[5:7], 16)
        h, l, s = colorsys.rgb_to_hls(r/255.0, g/255.0, b/255.0)
        
        # --- Shadow Logic ---
        s_shadow = min(1.0, s * 1.2)
        l_shadow = max(0.1, l * 0.6)
        h_shadow = h
        if 0.0 < h < 0.15: 
             h_shadow = max(0.0, h - 0.02) 
        r_s, g_s, b_s = colorsys.hls_to_rgb(h_shadow, l_shadow, s_shadow)
        shadow_hex = '#{:02x}{:02x}{:02x}'.format(int(r_s*255), int(g_s*255), int(b_s*255))

        # --- Highlight Logic ---
        s_highlight = max(0.0, s * 0.8)
        l_highlight = min(1.0, l * 1.3)
        h_highlight = h
        if h < 0.13:
            h_highlight = min(0.13, h + 0.02)
        elif h > 0.13:
            h_highlight = max(0.13, h - 0.02)
        r_h, g_h, b_h = colorsys.hls_to_rgb(h_highlight, l_highlight, s_highlight)
        highlight_hex = '#{:02x}{:02x}{:02x}'.format(int(r_h*255), int(g_h*255), int(b_h*255))

        outline_hex = "#202020"

        # --- Fallback Colors ---
        fallback_colors = {
            "White": "#FFFFFF", "Black": "#000000", "Grey": "#808080",
            "Red": "#FF0000", "Green": "#00FF00", "Blue": "#0000FF",
            "Yellow": "#FFFF00", "Orange": "#FFA500", "Purple": "#800080",
            "Brown": "#A52A2A"
        }
        
        fallback_str = ", ".join([f"{k} ({v})" for k, v in fallback_colors.items()])

        # --- Prompt Generation ---
        prompt = (
            f"Create a high-quality 2D game asset in a plush, tactile style. "
            f"The object has thick black outlines ({outline_hex}) and soft, rounded shading. "
            f"Lighting is golden and warm. "
            f"PRIMARY PALETTE: Base Color: {base_hex}, Shadow Color: {shadow_hex}, Highlight Color: {highlight_hex}. "
            f"Use this palette as the absolute priority for the main object and style. "
            f"FALLBACK LOGIC: If the scene requires a color NOT in the palette (e.g. Blue for sky, Green for grass), "
            f"you may use these standard colors: {fallback_str}. "
            f"CRITICAL: When using fallbacks, do NOT use them 'raw'. BLEND them with the Golden/Warm style of the palette "
            f"to ensure they fit the aesthetic. For example, a blue sky should have a warm golden tint, not be cold blue. "
            f"The overall look should be cute, vibrant, and high-resolution."
        )

        return {
            "base": base_hex,
            "shadow": shadow_hex,
            "highlight": highlight_hex,
            "outline": outline_hex,
            "fallbacks": fallback_colors,
            "prompt": prompt
        }

    def add_color_style(self, base_hex):
        if not self.current_image_path:
            return
            
        filename = os.path.basename(self.current_image_path)
        
        if filename not in self.palettes:
            self.palettes[filename] = []
            
        style_obj = self.generate_style(base_hex)

        if self.palettes[filename] and self.palettes[filename][-1]['base'] == base_hex:
            return

        self.palettes[filename].append(style_obj)
        self.save_palettes()
        self.refresh_color_list()

    def clear_colors(self):
        if not self.current_image_path:
            return
            
        filename = os.path.basename(self.current_image_path)
        if filename in self.palettes:
            self.palettes[filename] = []
            self.save_palettes()
            self.refresh_color_list()

    def refresh_color_list(self):
        for widget in self.scrollable_frame.winfo_children():
            widget.destroy()
        
        if not self.current_image_path:
            return
            
        filename = os.path.basename(self.current_image_path)
        styles = self.palettes.get(filename, [])
        
        for i, style in enumerate(styles):
            frame = tk.Frame(self.scrollable_frame, pady=5, bg="#f0f0f0")
            frame.pack(fill=tk.X)
            
            # Swatches
            swatch_frame = tk.Frame(frame, bg="#f0f0f0")
            swatch_frame.pack(side=tk.LEFT)
            self.create_swatch(swatch_frame, style['base'], "Base")
            self.create_swatch(swatch_frame, style['shadow'], "Shadow")
            self.create_swatch(swatch_frame, style['highlight'], "Highlt")
            
            # Prompt Button
            btn_copy = tk.Button(frame, text="Copy Prompt", command=lambda p=style['prompt']: self.copy_to_clipboard(p), font=("Arial", 9))
            btn_copy.pack(side=tk.RIGHT, padx=10)
            
            # Separator
            tk.Frame(self.scrollable_frame, height=1, bg="#cccccc").pack(fill=tk.X, pady=2)

    def create_swatch(self, parent, color, label):
        container = tk.Frame(parent, bg="#f0f0f0")
        container.pack(side=tk.LEFT, padx=5)
        
        lbl = tk.Label(container, text=label, font=("Arial", 8), bg="#f0f0f0")
        lbl.pack()
        
        swatch = tk.Label(container, width=6, height=2, bg=color, relief="solid", borderwidth=1)
        swatch.pack()
        
        hex_lbl = tk.Label(container, text=color, font=("Courier", 8), bg="#f0f0f0")
        hex_lbl.pack()

    def copy_to_clipboard(self, text):
        self.root.clipboard_clear()
        self.root.clipboard_append(text)
        messagebox.showinfo("Copied", "Prompt copied to clipboard!")

if __name__ == "__main__":
    root = tk.Tk()
    app = ColorPickerApp(root)
    root.mainloop()
