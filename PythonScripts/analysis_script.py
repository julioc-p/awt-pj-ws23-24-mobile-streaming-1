# analysis_script.py

import os
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns

def process_data(file_path):
    df = pd.read_csv(file_path, delimiter=';')
    avg_power = df[df['VideoPlaying'] == True]['TotalPower'].mean()
    return avg_power

def process_directory(directory_path, filename_condition):
    data_combinations = set()
    sum_powers = {}

    for filename in os.listdir(directory_path):
        if filename.startswith('random'):
            file_path = os.path.join(directory_path, filename)
            combination = filename_condition(file_path)
            data_combinations.add(combination)

            sum_power, num_data_points = process_file(file_path)
            if combination not in sum_powers:
                sum_powers[combination] = []
            sum_powers[combination].append([sum_power, num_data_points])

    return data_combinations, sum_powers

def process_file(file_path):
    df = pd.read_csv(file_path, delimiter=';')
    sum_power = df['TotalPower'].sum()
    return sum_power, len(df)

def plot_bitrate_vs_power(codec, bitrates, sum_powers):
    for bitrate in bitrates:
        sum_bitrate = 0
        num_data_points = 0
        for power, data_points in sum_powers[(codec, bitrate)]:
            sum_bitrate += power
            num_data_points += data_points
        plt.bar(bitrate, (sum_bitrate / num_data_points), label=f'{bitrate}')

    plt.xlabel('Bitrate')
    plt.ylabel('Average Power Consumption')
    plt.title(f'Average Power Consumption for {codec} at Different Bitrates')
    plt.legend()
    plt.show()

def plot_avg_power(data_combinations, sum_powers, x_label, plot_title):
    data_combinations = sorted(data_combinations)
    for combination in data_combinations:
        sum_combination = 0
        num_data_points = 0
        for power, data_points in sum_powers[combination]:
            sum_combination += power
            num_data_points += data_points
        plt.bar(f'{combination}', (sum_combination / num_data_points), label=f'{combination}')

    plt.xlabel(x_label)
    plt.ylabel('Average Power Consumption')
    plt.title(plot_title)
    plt.legend()
    plt.show()


def generate_insight_by_settings(data_directory):
    def filename_condition(filename):
        settings = filename.split('_')[1:]
        return '_'.join(settings)

    # Process files in the given directory
    settings_combinations, sum_powers = process_directory(data_directory, filename_condition)

    # Plot the average power for each combination of settings
    plot_avg_power(settings_combinations, sum_powers, 'Settings (Codec_Resolution_Bitrate_FPS)',
                   'Average Power Consumption for Different Settings')

def generate_insight_by_resolution(data_directory):
    def filename_condition(filename):
        return filename.split('_')[1]

    # Process files in the given directory
    resolutions, sum_powers = process_directory(data_directory, filename_condition)

    # Plot the average power for each resolution
    plot_avg_power(resolutions, sum_powers, 'Resolution',
                   'Average Power Consumption for Different Resolutions')


def generate_insight_by_codec_bitrate(data_directory):
    def filename_condition(filename):
        codec, bitrate = filename.split('_')[3:5]
        return f'{codec} x {bitrate}'

    # Process files in the given directory
    codec_bitrate_combinations, sum_powers = process_directory(data_directory, filename_condition)

    # Plot the average power for each combination of codec and bitrate
    plot_avg_power(codec_bitrate_combinations, sum_powers, 'Codec x Bitrate',
                   'Average Power Consumption for Different Codec x Bitrate Combinations')

# write a method to generate insights by bitrate
def generate_insight_by_bitrate(data_directory):
    def filename_condition(filename):
        return filename.split('_')[3]

    # Process files in the given directory
    bitrates, sum_powers = process_directory(data_directory, filename_condition)

    # Plot the average power for each resolution
    plot_avg_power(bitrates, sum_powers, 'Resolution',
                   'Average Power Consumption for Different Bitrates')

if __name__ == "__main__":
    # Path to the directory containing the data files
    data_directory = '../Measurements/data'

    # Generate insights
    generate_insight_by_settings(data_directory)
    generate_insight_by_resolution(data_directory)
    generate_insight_by_codec_bitrate(data_directory)
    generate_insight_by_bitrate(data_directory)
