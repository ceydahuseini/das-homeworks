import requests
import pandas as pd
from datetime import datetime
from io import StringIO
import os
start_time = time.time()
def fetch_missing_data(issuer_code, last_date):
    
    if isinstance(last_date, str):
        last_date = pd.to_datetime(last_date, dayfirst=True)

    last_date_str = last_date.strftime("%d.%m.%Y")
    end_date_str = datetime.now().strftime("%d.%m.%Y")

    url = f"https://www.mse.mk/mk/stats/symbolhistory/{issuer_code}"
    params = {
        "from": last_date_str,
        "to": end_date_str
    }

    
    headers = {
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
    }

    response = requests.get(url, params=params, headers=headers)

    
    try:
        tables = pd.read_html(StringIO(response.text))
        
        df = tables[0]
    except ValueError:
        print(f"No tables found for {issuer_code}. Check the response content.")
        print(response.text[:500])
        return

    
    df.columns = [
        "Датум", "Цена на последна трансакција", "Мак.", "Мин.",
        "Просечна цена", "%пром.", "Количина", "Промет во БЕСТ во денари",
        "Вкупен промет во денари"
    ]

    
    df['Датум'] = pd.to_datetime(df['Датум'], dayfirst=True, errors='coerce')

    
    os.makedirs("data", exist_ok=True)

    
    try:
        existing_df = pd.read_csv(f'data/{issuer_code}_data.csv')
        existing_df['Датум'] = pd.to_datetime(existing_df['Датум'], dayfirst=True, errors='coerce')
        combined_df = pd.concat([existing_df, df]).drop_duplicates(subset=['Датум']).sort_values('Датум')
    except FileNotFoundError:
        combined_df = df

    combined_df.to_csv(f'data/{issuer_code}_data.csv', index=False, encoding='utf-8-sig')
    print(f"Data for {issuer_code} saved to data/{issuer_code}_data.csv")


last_dates_df = pd.read_csv('last_dates.csv', names=['Issuer Code', 'Last Date'], parse_dates=['Last Date'])
for _, row in last_dates_df.iterrows():
    
    fetch_missing_data(row['Issuer Code'], row['Last Date'])

end_time = time.time()
execution_time = end_time - start_time
print(f"Total execution time: {execution_time:.2f} seconds")