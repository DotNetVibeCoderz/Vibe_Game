from PIL import Image, ImageDraw
import os

def ensure_dir(file_path):
    directory = os.path.dirname(file_path)
    if not os.path.exists(directory):
        os.makedirs(directory)

def create_texture(name, color, size=(32, 32), noise_color=None):
    img = Image.new('RGBA', size, color)
    draw = ImageDraw.Draw(img)
    if noise_color:
        # Simple noise
        import random
        for _ in range(20):
            x = random.randint(0, size[0]-1)
            y = random.randint(0, size[1]-1)
            draw.point((x, y), fill=noise_color)
    img.save(f"Assets/{name}.png")

def create_chicken():
    img = Image.new('RGBA', (32, 32), (0, 0, 0, 0)) # Transparent
    draw = ImageDraw.Draw(img)
    
    # Body (White)
    draw.rectangle([8, 8, 24, 24], fill=(255, 255, 255))
    # Comb (Red)
    draw.rectangle([12, 4, 20, 8], fill=(255, 0, 0))
    # Beak (Orange)
    draw.rectangle([24, 12, 28, 16], fill=(255, 165, 0))
    # Eye (Black)
    draw.rectangle([20, 10, 22, 12], fill=(0, 0, 0))
    # Legs (Orange)
    draw.rectangle([10, 24, 12, 28], fill=(255, 165, 0))
    draw.rectangle([20, 24, 22, 28], fill=(255, 165, 0))
    
    img.save("Assets/chicken.png")

def create_car(name, color):
    img = Image.new('RGBA', (60, 32), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Body
    draw.rectangle([4, 8, 56, 24], fill=color)
    # Windows
    draw.rectangle([10, 10, 50, 22], fill=(200, 240, 255))
    # Wheels
    draw.rectangle([8, 22, 16, 30], fill=(0, 0, 0))
    draw.rectangle([44, 22, 52, 30], fill=(0, 0, 0))
    
    img.save(f"Assets/{name}.png")

def create_log():
    img = Image.new('RGBA', (100, 32), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Wood
    draw.rectangle([0, 4, 100, 28], fill=(139, 69, 19))
    # Detail lines
    draw.line([10, 4, 10, 28], fill=(101, 67, 33), width=2)
    draw.line([50, 4, 50, 28], fill=(101, 67, 33), width=2)
    draw.line([90, 4, 90, 28], fill=(101, 67, 33), width=2)
    
    img.save("Assets/log.png")

def create_train():
    img = Image.new('RGBA', (800, 32), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Train Body
    draw.rectangle([0, 2, 800, 30], fill=(80, 80, 80))
    # Stripe
    draw.rectangle([0, 10, 800, 14], fill=(255, 0, 0))
    # Windows
    for i in range(0, 800, 40):
        draw.rectangle([i+10, 5, i+30, 15], fill=(200, 240, 255))
        
    img.save("Assets/train.png")

def create_rail():
    img = Image.new('RGBA', (32, 32), (210, 180, 140)) # Sandy ground
    draw = ImageDraw.Draw(img)
    
    # Sleepers
    draw.rectangle([4, 0, 8, 32], fill=(101, 67, 33))
    draw.rectangle([24, 0, 28, 32], fill=(101, 67, 33))
    
    # Rails (Horizontal)
    draw.rectangle([0, 8, 32, 10], fill=(169, 169, 169))
    draw.rectangle([0, 22, 32, 24], fill=(169, 169, 169))
    
    img.save("Assets/rail.png")

if __name__ == "__main__":
    ensure_dir("Assets/chicken.png")
    
    create_chicken()
    create_texture("grass", (34, 139, 34), noise_color=(50, 205, 50))
    create_texture("road", (60, 60, 60), noise_color=(80, 80, 80))
    create_texture("water", (0, 191, 255), noise_color=(135, 206, 250))
    create_rail()
    create_car("car_red", (255, 0, 0))
    create_log()
    create_train()
    print("Assets generated.")