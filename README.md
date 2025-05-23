# Практична робот № 1.4
## Розв’язання задачі цілочислового лінійного програмування

### ПОСТАНОВКА ЗАВДАННЯ
Під час виконання практичної роботи необхідно розробити програму, за допомогою якої на основі методу Гоморі можна буде отримати цілочисловий розв’язок задачі лінійного програмування.  
Необхідно описати структуру програми, вхідні й вихідні дані, навести екранні форми (screenshots) програми, а також продемонструвати виконання як тестових прикладів, так і власного варіанта завдання.

#### Варіант 20

Задача лінійного програмування для варіанту 20:  

Z = 3x1 – x3 + x5 → min  
при обмеженнях:  
– x1 + 3x3 – 2x4 + x5 <= 3;  
x1 – x2 + x4 + x5 <= 3;  
x1 + 3x2 – x3 – x4 + x5 <=2;  
xj >= 0, j = 1, 5  

### ВИКОНАННЯ РОБОТИ

#### Опис програми

На мові програмування C# розроблено консольний додаток для розв’язання задачі цілочисельного лінійного програмування методом «симплекс + відсікання Ґоморі». Програма автоматично визначає, чи це задача на максимум чи мінімум, на основі введеної форми цільової функції, будує початкову симплекс-таблицю, послідовно виконує жорданові виключення для пошуку опорного розв’язку, потім – симплекс-фазу для оптимізації, а у випадку неповних (дробових) значень у базисі – додає відповідні відскання Ґоморі та повторює обчислення. Усі ключові кроки (побудова таблиці, пошук опорного та оптимального розв’язків, додавання відсічок) виводяться у консоль.

#### Вхідні дані
•	objectiveFunction (string) – рядок із цільовою функцією у форматі, наприклад, 2x1 + 3x2 - x3 -> max або -2x1 + 5x2 -> min.  
•	constraints (List<string>) – список обмежень у форматі нерівностей, наприклад, x1 + 2x2 - x3 <= 5 або 3x1 - x2 + x4 >= 4.  
•	variableCount (int) – кількість змінних n.  
•	isMinimization (bool) – внутрішня змінна, що встановлюється під час розбору objectiveFunction (true – мінімізація, false – максимізація).  

#### Вихідні дані
•	simplexTable (object[,]) – поточна симплекс-таблиця з назвами базисних змінних, коефіцієнтами при xj та вільними членами.  
•	X (double[]) – масив значень x1,…,xn оптимального цілочисельного розв’язку.  
•	Z (double) – значення цільової функції в точці оптимуму.  
•	intermediateSteps – послідовність симплекс-таблиць, що виводяться в консоль:  
-	початкова нормалізована таблиця,  
-	таблиці під час пошуку опорного розв’язку,  
-	таблиці під час оптимізаційної фази,  
-	таблиці після кожного додавання відсікання Ґоморі.
  
•	messages – текстові повідомлення в консоль, які інформують про:  
  -	початок пошуку опорного розв’язку,  
-	знайдений опорний розв’язок,  
-	початок та завершення симплекс-фази,  
-	невизначеність задачі (якщо функція необмежена),  
-	додавання відсікання Ґоморі,  
-	знайдений цілочисельний оптимальний розв’язок.

#### Екранні форми роботи програми

На основі задачі лінійного програмування для варіанту 20 було проведено тестування програми, протоколи проміжних обчислень та їх результати зображено на рисунках 1 та 2.

![image](https://github.com/user-attachments/assets/566c8d97-7f64-44fa-ba81-32878f03e467)  
Рисунок 1 – Тестування програми на задачі для варіанту 20


![image](https://github.com/user-attachments/assets/e7afc263-ffb0-485e-afc7-dd070793a1b3)  
Рисунок 2 – Тестування програми на задачі для варіанту 20 (продовження)

Також програма була перевірена на тестових прикладах. Тестовий приклад 1 наведено на рисунку 3. Результат тестування програми наведено на рисунках 4 та 5.

![image](https://github.com/user-attachments/assets/9cc802b5-2d17-41e0-b2f0-e55c7aebfae1)  
Рисунок 3 – Тестовий приклад 1

![image](https://github.com/user-attachments/assets/3c614e4c-20b3-4fa6-8946-59b69d65c534)  
Рисунок 4 – Тестування на прикладі 1

![image](https://github.com/user-attachments/assets/08a4e696-8463-4b8d-85c6-a8685d6c9727)  
Рисунок 5 – Тестування на прикладі 1 (продовження)

Тестовий приклад 2 наведено на рисунку 6. Результат тестування програми наведено на рисунках 7-10.

![image](https://github.com/user-attachments/assets/38abb941-582e-4904-ab1f-ab78c41c9735)  
Рисунок 6 – Тестовий приклад 2

![image](https://github.com/user-attachments/assets/8b7404ed-d855-4092-8865-81afcea2fe54)  
Рисунок 7 – Тестування на прикладі 2 (частина 1)

![image](https://github.com/user-attachments/assets/a9b3acfc-d7ac-44e0-94cb-25bafea7af2f)  
Рисунок 8 – Тестування на прикладі 2 (частина 2)

![image](https://github.com/user-attachments/assets/171af27f-8fba-4b92-a53f-43b73ea090d2)  
Рисунок 9 – Тестування на прикладі 2 (частина 3)

![image](https://github.com/user-attachments/assets/8b98774c-87d8-4585-b3cf-bc8484cb8b00)  
Рисунок 10 – Тестування на прикладі 2 (частина 4)

### ВИСНОВОК

У ході виконання роботи було створено консольний додаток на C#, який реалізує розв’язання задач лінійного програмування симплекс-методом із відсіканнями Ґоморі для цілочисельних рішень. Програма автоматично визначає режим (максимум чи мінімум), перетворюючи мінімізацію в еквівалентну задачу на максимум, і забезпечує інтерактивний ввід даних. Вивід проміжних симплекс-таблиць і фінального рішення дає змогу наочно простежити хід обчислень. Отримані результати відповідають аналітичним прикладам і демонструють коректність та практичну придатність реалізованого алгоритму.
