# analysis_script.py

import os
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
from textwrap import wrap

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

def process_directory_all(directory_path, filename_condition):
    data_combinations = set()
    sum_powers = {}

    for directory in os.listdir(directory_path):
        directory_path_aux = os.path.join(directory_path, directory)
        for filename in os.listdir(directory_path_aux):
            file_path = os.path.join(directory_path_aux, filename)
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
    margin = 0.1 
    values = avg_powers.values()
    y_min = min(values) - margin * (max(values) - min(values) + 0.1)
    y_max = max(values) + margin * (max(values) - min(values) + 0.1)
    plt.ylim(y_min, y_max)
    plt.xlabel(x_label)
    plt.xticks(visible = False)
    plt.ylabel('Average Power Consumption')
    ax = plt.gca()
    box = ax.get_position()
    ax.set_position([box.x0, box.y0, box.width * 0.8, box.height])
    ax.set_title("\n".join(wrap(plot_title, 50)))
    lgd = plt.legend(loc='center left', bbox_to_anchor=(1, 0.5))
    # turn plot into a png file
    directory_name = directory_name if directory_name else 'overall'
    plt.savefig(f'{directory_name}_{plot_title}.png', bbox_inches='tight')
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
    plt.title('Correlation Heat Map')
    # turn plot into a png file
    plt.savefig(f'{directory_name}_correlation_heat_map.png')
    plt.clf()

def generate_insight_by_settings(data_directory, directory_name = None):
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
        settings[3] = normalize_codec(settings[3])
        return '_'.join(settings)

    # Process files in the given directory
    settings_combinations, sum_powers = process_directory(data_directory, filename_condition) if directory_name else process_directory_all(data_directory, filename_condition)
    avg_powers = get_avg_power(settings_combinations, sum_powers)
    # Plot the average power for each combination of settings
    plot_avg_power(avg_powers, 'Settings (Codec_Resolution_Bitrate_FPS)',
                   'Average Power Consumption for Different Settings', directory_name)
    # create a dataframe with one column per setting and one column for the average powe
    df = create_dataframe(avg_powers)
    plot_corr_heat_map(df, directory_name)

    

def generate_insight_by_resolution(data_directory, directory_name = None):
    def filename_condition(filename):
        return filename.split('_')[1]

    # Process files in the given directory
    resolutions, sum_powers = process_directory(data_directory, filename_condition) if directory_name else process_directory_all(data_directory, filename_condition)
    avg_powers = get_avg_power(resolutions, sum_powers)

    # Plot the average power for each resolution
    plot_avg_power(avg_powers, 'Resolution',
                   'Average Power Consumption for Different Resolutions', directory_name)


def generate_insight_by_codec_bitrate(data_directory, directory_name = None):
    def filename_condition(filename):
        codec, bitrate = filename.split('_')[3:5]
        bitrate = normalize_codec(bitrate)
        return f'{codec} x {bitrate}'

    # Process files in the given directory
    codec_bitrate_combinations, sum_powers = process_directory(data_directory, filename_condition) if directory_name else process_directory_all(data_directory, filename_condition)
    avg_powers = get_avg_power(codec_bitrate_combinations, sum_powers)

    # Plot the average power for each combination of codec and bitrate
    plot_avg_power(avg_powers, 'Bitrate x Codec',
                   'Average Power Consumption for Different Bitrate x Codec Combinations', directory_name)

# write a method to generate insights by bitrate
def generate_insight_by_bitrate(data_directory, directory_name = None):
    def filename_condition(filename):
        return filename.split('_')[3]

    # Process files in the given directory
    bitrates, sum_powers = process_directory(data_directory, filename_condition) if directory_name else process_directory_all(data_directory, filename_condition)
    avg_powers = get_avg_power(bitrates, sum_powers)

    plot_avg_power(avg_powers, 'Bitrate',
                   'Average Power Consumption for Different Bitrates', directory_name)

def generate_insights_by_codec(data_directory, directory_name = None):
    def filename_condition(filename):
        return normalize_codec(filename.split('_')[4])

    # Process files in the given directory
    codecs, sum_powers = process_directory(data_directory, filename_condition) if directory_name else process_directory_all(data_directory, filename_condition)
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

def generate_overall_insights(data_directory):
            generate_insight_by_settings(data_directory)
            generate_insight_by_resolution(data_directory)
            generate_insight_by_codec_bitrate(data_directory)
            generate_insight_by_bitrate(data_directory)
            generate_insights_by_codec(data_directory)

if __name__ == "__main__":
    # Path to the directory containing the data files
    data_directory = "Measurements/data"

    # Generate insights
    generate_insight_for_directories(data_directory)
    generate_overall_insights(data_directory)