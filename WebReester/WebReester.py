import requests
import json

with open("CheckThisModel.json") as file:
    dictData = json.load(d)
    check_status = requests.get(
        f'https://kkt-online.nalog.ru/lkip.html?query=/kkt/model/check&factory_number={dictData["FactoryNumber"]}&model_code={dictData["model"]}').json()
    if check_status['status']:
        if check_status['check_status']:
            res = 1
        else:
            res = 0
with open('result.json', 'w') as fp:
    json.dump({"result": res}, fp)
