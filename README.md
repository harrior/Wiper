# Wiper<br>
Simple tool for data wiping<br>
Это простая утилита для удаления файлов и папок без возможности программного восстановления. <br>
Для этого перед удалением целевой файл перезаписывается (реализована однократная перезапись случайными числами, DoD 5220.22-M и метод Шнайдера (по-умолчанию)). В качестве дополнительной меры перед удалением обнуляется размер файла и генерируется случайное имя. Программа не имеет графического интерфейса и вызывается из консоли или контекстного меню.<br>
Опции командной строки:<br>
-s Вывод диалога настройки алгоритма удаления по умолчанию.<br>
-i добавляет утилиту в контекстное меню<br>
-u удаляет утилиту из контекстного меню<br>
-m(0-2) выбор метода удаления (0 - random, 1 - DoD 5220.22-M,2 -Шнайдер )<br>
-h скрытый режим, программа удаляет файлы не запрашивая подтверждения и не сообщая об ошибках<br>
