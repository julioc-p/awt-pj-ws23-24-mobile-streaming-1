import os
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns

# Path to the directory containing the data files
data_directory = '../Measurements/data'

# Function to read and process a single data file
def process_data(file_path):
    df = pd.read_csv(file_path, delimiter=';')
    avg_power = df[df['VideoPlaying'] == True]['TotalPower'].mean()
    return avg_power

# Process all data files in the directory
all_data = []
for filename in os.listdir(data_directory):
    if filename.startswith('random'):
        file_path = os.path.join(data_directory, filename)
        settings = filename.split('_')[1:]  # Extracting codec, resolution, bitrate, fps from filename
        settings_str = '_'.join(settings)
        avg_power = process_data(file_path)
        all_data.append({'Settings': settings_str, 'TotalPower': avg_power})

# Create DataFrame from the list of dictionaries
result_df = pd.DataFrame(all_data)

# Extract individual settings into separate columns
result_df[['Codec', 'Resolution', 'Bitrate', 'FPS']] = result_df['Settings'].str.split('_', expand=True)

# Define custom sorting order for 'Resolution' column
resolution_order = ['HD', '4K']
result_df['Resolution'] = pd.Categorical(result_df['Resolution'], categories=resolution_order, ordered=True)

# Sort DataFrame by 'Resolution' and 'Bitrate'
result_df = result_df.sort_values(by=['Resolution', 'Bitrate'])

# Plotting using seaborn
plt.figure(figsize=(16, 8))  # Increase the width of the plot
sns.barplot(x='Settings', y='TotalPower', data=result_df)
plt.title('Average Power Consumption for Different Settings')
plt.xlabel('Settings (Codec_Resolution_Bitrate_FPS)')
plt.ylabel('Average Power Consumption')
plt.xticks(rotation=45, ha='right')  # Adjust rotation and horizontal alignment
plt.tight_layout()  # Ensure proper layout
plt.show()
