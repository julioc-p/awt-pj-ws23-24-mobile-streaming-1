import os
import pandas as pd
import matplotlib.pyplot as plt

# Assuming all combination files are in the same folder
folder_path = '../Measurements/data'

# List all files in the folder
files = [f for f in os.listdir(folder_path) if f.startswith('measurement_') and f.endswith('.txt')]

# Create an empty DataFrame to store the data
all_data = []

# Read each file and append the data to the DataFrame
for file in files:
    data = pd.read_csv(os.path.join(folder_path, file), sep=";")
    
    # Extract codec and resolution from the file name
    _, timestamp, resolution, codec = file.split('_')
    
    # Create new columns for codec and resolution
    data['Codec'] = codec.split('.')[0]
    data['Resolution'] = resolution.strip('()')  # Remove parentheses
    data['Timestamp'] = pd.to_datetime(data['Timestamp'])  # Convert 'Timestamp' to datetime format
    
    # Append data to the list
    all_data.append(data)

# Concatenate all data into a single DataFrame
df = pd.concat(all_data, ignore_index=True)

# Order resolutions on the x-axis
resolutions_order = ['512x288', '640x360', '852x480', '1820x720', '1920x1080']

# Convert 'Resolution' to a categorical type with the specified order
df['Resolution'] = pd.Categorical(df['Resolution'], categories=resolutions_order, ordered=True)

# Group by codec and resolution, calculate the average power consumption
grouped_data = df.groupby(['Codec', 'Resolution']).mean().reset_index()

# Visualization: Average Power Consumption by Codec and Resolution
plt.figure(figsize=(12, 6))
for codec, group in grouped_data.groupby('Codec'):
    plt.plot(group['Resolution'], group['TotalPower'], label=codec, marker='o')

plt.xlabel('Resolution')
plt.ylabel('Average Power Consumption')
plt.title('Average Power Consumption by Codec and Resolution')
plt.legend()
plt.xticks(rotation=45)  # Rotate labels
plt.show()
