# analysis_script.py

import os
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns

def process_directory(directory_path, filename_condition):
    data_combinations = set()
    sum_powers = {}

    for filename in os.listdir(directory_path):
        file_path = os.path.join(directory_path, filename)
        combination = filename_condition(filename)
        data_combinations.add(combination)

        sum_power, num_data_points = process_file(file_path)
        if combination not in sum_powers:
            sum_powers[combination] = []
        sum_powers[combination].append([sum_power, num_data_points])

    return data_combinations, sum_powers

def normalize_codec(codec):
    if codec.startswith('avc'):
        return 'H264'
    elif codec.startswith('hev'):
        return 'H265'
    else:
        return codec
    
def get_avg_power(data_combinations, sum_powers):
    avg_powers = {}
    data_combinations = sorted(data_combinations)
    for combination in data_combinations:
        sum_combination = 0
        num_data_points = 0
        for power, data_points in sum_powers[combination]:
            sum_combination += power
            num_data_points += data_points
        avg_powers[combination] = sum_combination / num_data_points
    return avg_powers

def process_file(file_path):
    df = pd.read_csv(file_path, delimiter=';')
    if len(df) == 0:
        raise Exception("No data in the file")
    if type(df['TotalPower'][0]) == str:
        df['TotalPower'] = df['TotalPower'].str.replace(',', '.').astype(float)
    sum_power = df['TotalPower'].sum()
    return sum_power, len(df)

def plot_avg_power(avg_powers, x_label, plot_title, directory_name):
    for combination in avg_powers:
        bars = plt.bar(f'{combination}', avg_powers[combination], label=f'{combination}')
        for bar in bars:
            height = bar.get_height()
            plt.annotate(f'{round(height, 2)}', xy=(bar.get_x() + bar.get_width() / 2, height), xytext=(0, 3),
            textcoords="offset points", ha='center', va='bottom')

    plt.xlabel(x_label)
    plt.xticks(visible = False)
    plt.ylabel('Average Power Consumption')
    plt.title(plot_title)
    plt.legend()
    # turn plot into a png file
    plt.savefig(f'{directory_name}_{plot_title}.png')
    plt.clf()

def create_dataframe(avg_powers):
    l = []
    for key, value in avg_powers.items():
        l_tmp = []
        l_tmp.extend(key.split('_')[:4])
        l_tmp.append(value)
        l.append(l_tmp)
    df = pd.DataFrame(l, columns=['Resolution', 'FPS', 'Bitrate', 'Codec', 'Average Power'])
    df["Resolution"] = df["Resolution"].astype('category').cat.codes
    df["FPS"] = df["FPS"].astype('category').cat.codes
    df["Bitrate"] = df["Bitrate"].astype('category').cat.codes
    df["Codec"] = df["Codec"].astype('category').cat.codes
    return df

def plot_corr_heat_map(df, directory_name):
    corr = df.corr()
    # get the last column of the correlation matrix for the avg power

    corr = corr.iloc[[4]]
    corr = corr.abs()
    #plt.matshow(corr)
    sns.heatmap(corr, annot=True, fmt="g", cmap='viridis')
    # turn plot into a png file
    plt.savefig(f'{directory_name}_correlation_heat_map.png')
    plt.clf()

def generate_insight_by_settings(data_directory, directory_name):
    """
    Generates insights based on the settings of the data files in the given directory.

    Args:
        data_directory (str): The path to the directory containing the data files.
        directory_name (str): The name of the directory.

    Returns:
        None
    """
    def filename_condition(filename):
        settings = filename.split('_')[1:5]
        return '_'.join(settings)

    # Process files in the given directory
    settings_combinations, sum_powers = process_directory(data_directory, filename_condition)
    avg_powers = get_avg_power(settings_combinations, sum_powers)
    # Plot the average power for each combination of settings
    plot_avg_power(avg_powers, 'Settings (Codec_Resolution_Bitrate_FPS)',
                   'Average Power Consumption for Different Settings', directory_name)
    # create a dataframe with one column per setting and one column for the average powe
    df = create_dataframe(avg_powers)
    plot_corr_heat_map(df, directory_name)

    

def generate_insight_by_resolution(data_directory, directory_name):
    def filename_condition(filename):
        return filename.split('_')[1]

    # Process files in the given directory
    resolutions, sum_powers = process_directory(data_directory, filename_condition)
    avg_powers = get_avg_power(resolutions, sum_powers)

    # Plot the average power for each resolution
    plot_avg_power(avg_powers, 'Resolution',
                   'Average Power Consumption for Different Resolutions', directory_name)


def generate_insight_by_codec_bitrate(data_directory, directory_name):
    def filename_condition(filename):
        codec, bitrate = filename.split('_')[3:5]
        return f'{codec} x {bitrate}'

    # Process files in the given directory
    codec_bitrate_combinations, sum_powers = process_directory(data_directory, filename_condition)
    avg_powers = get_avg_power(codec_bitrate_combinations, sum_powers)

    # Plot the average power for each combination of codec and bitrate
    plot_avg_power(avg_powers, 'Bitrate x Codec',
                   'Average Power Consumption for Different Bitrate x Codec Combinations', directory_name)

# write a method to generate insights by bitrate
def generate_insight_by_bitrate(data_directory, directory_name):
    def filename_condition(filename):
        return filename.split('_')[3]

    # Process files in the given directory
    bitrates, sum_powers = process_directory(data_directory, filename_condition)
    avg_powers = get_avg_power(bitrates, sum_powers)

    plot_avg_power(avg_powers, 'Bitrate',
                   'Average Power Consumption for Different Bitrates', directory_name)

def generate_insights_by_codec(data_directory, directory_name):
    def filename_condition(filename):
        return normalize_codec(filename.split('_')[4])

    # Process files in the given directory
    codecs, sum_powers = process_directory(data_directory, filename_condition)
    avg_powers = get_avg_power(codecs, sum_powers)

    plot_avg_power(avg_powers, 'Codec',
                   'Average Power Consumption for Different Codecs', directory_name)
# generate insights for all the directories in the measuredata directory
def generate_insight_for_directories(data_directory):
    for directory in os.listdir(data_directory):
        directory_path = os.path.join(data_directory, directory)
        if os.path.isdir(directory_path):
            print(f'Generating insights for {directory}')
            generate_insight_by_settings(directory_path, directory)
            generate_insight_by_resolution(directory_path, directory)
            generate_insight_by_codec_bitrate(directory_path, directory)
            generate_insight_by_bitrate(directory_path, directory)
            generate_insights_by_codec(directory_path, directory)


if __name__ == "__main__":
    # Path to the directory containing the data files
    data_directory = "Measurements/data"

    # Generate insights
    generate_insight_for_directories(data_directory)
    # generate_insight_by_settings(data_directory)
    # generate_insight_by_resolution(data_directory)
    # generate_insight_by_codec_bitrate(data_directory)
    # generate_insight_by_bitrate(data_directory)