import requests
from bs4 import BeautifulSoup
import pandas as pd


def get_issuer_codes():
    url = "https://www.mse.mk/mk/stats/symbolhistory/REPL"
    response = requests.get(url)
    soup = BeautifulSoup(response.content, 'html.parser')
    
    
    issuer_codes = []
    dropdown = soup.find('select')  
    for option in dropdown.find_all('option'):
        code = option.text.strip()
        if code.isalpha():
            issuer_codes.append(code)
    
    
    df_codes = pd.DataFrame(issuer_codes, columns=['Issuer Code'])
    df_codes.to_csv('issuer_codes.csv', index=False, encoding='utf-8-sig')
    print("Issuer codes saved to issuer_codes.csv")

    return issuer_codes

issuer_codes = get_issuer_codes()
