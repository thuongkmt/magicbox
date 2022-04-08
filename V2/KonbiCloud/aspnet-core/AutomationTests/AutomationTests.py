from selenium import webdriver

browser = webdriver.Firefox()
browser.get("http://localhost:4200/#/app/admin/machinesconfig")

assert "KonbiCloud" in browser.title

browser.close()