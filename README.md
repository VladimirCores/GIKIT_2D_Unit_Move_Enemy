# GIKIT_2D_Unit_Move_Enemy

Задание на зачет от Владимир Минкин (это я пишу, не Анастасия, которой Спасибо за аккаунт))
Для проекта с прошлого семестра, мини-игра в которой перемещается персонаж и стреляет по целям, нужно добавить "таблицу результатов" (Leaderboard):
Таблица показывает несколько колонок:
имя игрока, которое если введено (может быть "захардкожено" в коде или генерироваться случайным текстом при старте), если имени нет то "unknown"
время игры с начала пользовательского взаимодействия до момента уничтожения всех целей.
Таблица загружается из базы данных, (рекомендуется использовать GraphQL), когда игрок закончил игру и выиграл - т.е. его жизни больше 0 и все цели поражены (на экране текст WIN). 3. В таблице показывается только первые 10 результатов по увеличению времени игры. При этом в конце текущей игры результат игрока отправляется в базу данных и конечные данные таблицы могут содержать результат игрока (если он входит в первую 10-ку).