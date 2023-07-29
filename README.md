# CSV_Converter
Der CSV_Converter ist eine Testimplementierung eines Converters, welcher Daten aus einer Textdatei mit Komma-separiertem Inhalt zeilenweise ausliest, mit diesen Daten eine Geometrie erstellt und diese anschließend in einer Textdatei in einem vordefinierten (konfigurierbaren) Format abspeichert.
Die App stellt dafür eine Oberfläche zur Verfügung mit welcher man eine Datei einlesen und den konvertierten Inhalt in eine Datei abspeichern kann. Es wird desweiteren der Status inklusive einer Fehlerbeschreibung des letzten Schrittes angezeigt.

Zur Konfigurationsdatei:
Der Standarddatentyp eines Eingabe- bzw Ausgabeparameters ist "double" und muss nicht explizit angegeben werden.
Die Eingabe- bzw Ausgabeparameter werden entsprechend der Indizes und eindeutigen Namen ausgelesen/berechnet/gesetzt.
Über das Attribut "decimaldigits" kann man die Anzahl der Dezimalstellen eines Zahlenwerts festlegen. 

Anmerkung zu CON:
Damit man verschiedene Definitionen des Kegels mit einer Konfiguration abdecken kann, müssen die variabel auftretenden Parameter (hier: h1 und r2) als String definiert werden, da sonst das Einlesen der Parameter einen Fehler erzeugt, da "" kein zulässiger Double-Wert ist.
