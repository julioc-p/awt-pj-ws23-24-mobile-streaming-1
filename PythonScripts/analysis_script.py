# analysis_script.py

import os
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns

def process_data(file_path):
    df = pd.read_csv(file_path, delimiter=';')
    avg_power = df[df['VideoPlaying'] == True]['TotalPower'].mean()
    return avg_power

def generate_insight_by_settings(data_directory):
    all_data = []
    for filename in os.listdir(data_directory):
        if filename.startswith('random'):
            file_path = os.path.join(data_directory, filename)
            settings = filename.split('_')[1:]  # Extracting codec, resolution, bitrate, fps from filename
            settings_str = '_'.join(settings)
            avg_power = process_data(file_path)
            all_data.append({'Settings': settings_str, 'TotalPower': avg_power})

    if not all_data:
        print("No data found.")
        return

    # Create DataFrame from the list of dictionaries
    result_df = pd.DataFrame(all_data)

    # Extract individual settings into separate columns
    result_df[['Codec', 'Resolution', 'Bitrate', 'FPS']] = result_df['Settings'].str.split('_', expand=True)

    # Sort by settings for better visualization
    result_df = result_df.sort_values(by='Settings')

    # Plotting using seaborn
    plt.figure(figsize=(15, 8))  # Adjusted figure size
    sns.barplot(x='Settings', y='TotalPower', data=result_df)
    plt.title('Average Power Consumption for Different Settings')
    plt.xlabel('Settings (Codec_Resolution_Bitrate_FPS)')
    plt.ylabel('Average Power Consumption')
    plt.xticks(rotation=45, ha='right')  # Adjust rotation and alignment
    plt.tight_layout()  # Improved spacing
    plt.show()

def generate_insight_by_resolution(data_directory):
    def process_file(file_path):
        df = pd.read_csv(file_path, delimiter=';')
        sum_power = df['TotalPower'].sum()
        return sum_power, len(df)

    def process_directory(directory_path):
        resolutions = set()
        sum_powers = {}

        for filename in os.listdir(directory_path):
            if filename.startswith('random'):
                file_path = os.path.join(directory_path, filename)
                resolution = filename.split('_')[1]
                resolutions.add(resolution)

                sum_power, num_data_points = process_file(file_path)
                if resolution not in sum_powers:
                    sum_powers[resolution] = []
                sum_powers[resolution].append([sum_power, num_data_points])

        return resolutions, sum_powers

    def plot_avg_power(resolutions, sum_powers):
        for resolution in resolutions:
            sum_resolution = 0
            num_data_points = 0
            for power, data_points in sum_powers[resolution]:
                sum_resolution += power
                num_data_points += data_points
            plt.bar(resolution, (sum_resolution / num_data_points), label=resolution)

        plt.xlabel('Resolution')
        plt.ylabel('Average Power Consumption')
        plt.title('Average Power Consumption for Different Resolutions')
        plt.legend()
        plt.show()

    # Process files in the given directory
    resolutions, sum_powers = process_directory(data_directory)

    # Plot the average power for each resolution
    plot_avg_power(resolutions, sum_powers)

if __name__ == "__main__":
    # Path to the directory containing the data files
    data_directory = '../Measurements/data'

    # Generate insights
    generate_insight_by_settings(data_directory)
    generate_insight_by_resolution(data_directory)
