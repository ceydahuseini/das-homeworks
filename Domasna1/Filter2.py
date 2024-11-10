import pandas as pd
from datetime import datetime, timedelta

def get_last_date(issuer_code):
    try:
        
        df = pd.read_csv(f'data/{issuer_code}_data.csv')
        last_date = pd.to_datetime(df['Датум']).max()
        print(f"Last available date for {issuer_code}: {last_date}")
    except FileNotFoundError:
        
        last_date = datetime.now() - timedelta(days=10*365)
        print(f"No data for {issuer_code}. Starting from {last_date}")

    
    pd.DataFrame([[issuer_code, last_date]], columns=['Issuer Code', 'Last Date']).to_csv('last_dates.csv', mode='a', index=False, header=False)
    
    return last_date

for issuer in issuer_codes:
    get_last_date(issuer)
