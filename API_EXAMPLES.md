# TaskManager API Test Examples

Aceste sunt exemple de cereri pentru a testa API-ul TaskManager cu toate operațiile disponibile.

## Endpoint de bază
```
POST http://localhost:5000/api/tasks/process
Content-Type: application/json
```

## Operații disponibile:
- **CREATE** - Creează task nou
- **UPDATE** - Actualizează task existent (trimite notificare RabbitMQ fanout)
- **DELETE** - Șterge task
- **GETALL** - Lista toate task-urile
- **GETRUNNING** - Lista doar task-urile active (IsRunning = true)
- **SCHEDULE** - Programează task (RabbitMQ async → IsRunning = true)
- **UNSCHEDULE** - Oprește task (RabbitMQ async → IsRunning = false)

---


## 1. CREATE TASK
```json
{
  "action": "create",
  "createRequest": {
    "name": "Task de test",
    "description": "Acesta este un task de test"
  }
}
```

## 2. UPDATE TASK (cu notificare RabbitMQ)
```json
{
  "action": "update",
  "taskId": "db0fc72e-4075-4106-ab06-b82c95ba08f7",
  "updateRequest": {
    "name": "Task actualizat ",
    "description": "Descriere nouă pentru a testa notificările fanout"
  }
}
```

## 3. GET ALL TASKS
```json
{
  "action": "getall"
}
```

## 4. GET RUNNING TASKS (doar task-urile active)
```json
{
  "action": "getrunning"
}
```

## 5. SCHEDULE TASK (RabbitMQ async operation)
```json
{
  "action": "schedule",
  "scheduleRequest": {
    "taskId": "db0fc72e-4075-4106-ab06-b82c95ba08f7"
  }
}
```

## 6. UNSCHEDULE TASK (RabbitMQ async operation)
```json
{
  "action": "unschedule",
  "taskId": "db0fc72e-4075-4106-ab06-b82c95ba08f7"
}
```

## 7. DELETE TASK
```json
{
  "action": "delete",
  "taskId": "db0fc72e-4075-4106-ab06-b82c95ba08f7"
}
```

---

## TESTE BAZATE PE TASK-URI EXISTENTE

### **Task disponibil pentru testare:**
ID: `db0fc72e-4075-4106-ab06-b82c95ba08f7`

## TEST 1: VERIFICĂ TOATE TASK-URILE
```json
{
  "action": "getall"
}
```

## TEST 2: VERIFICĂ TASK-URILE ACTIVE (ar trebui să returneze task-ul de mai sus)
```json
{
  "action": "getrunning"
}
```

## TEST 3: UNSCHEDULE TASK-UL EXISTENT
```json
{
  "action": "unschedule",
  "taskId": "db0fc72e-4075-4106-ab06-b82c95ba08f7"
}
```

## TEST 4: VERIFICĂ CĂ TASK-UL NU MAI ESTE RUNNING
```json
{
  "action": "getrunning"
}
```

## TEST 5: UPDATE TASK-UL CU NUME ȘI DESCRIERE NOI
```json
{
  "action": "update",
  "taskId": "db0fc72e-4075-4106-ab06-b82c95ba08f7",
  "updateRequest": {
    "name": "Task Actualizat - Test Complet",
    "description": "Descriere actualizată pentru testarea completă a sistemului"
  }
}
```

## TEST 6: VERIFICĂ ACTUALIZAREA
```json
{
  "action": "getall"
}
```

## TEST 7: SCHEDULE DIN NOU TASK-UL
```json
{
  "action": "schedule",
  "scheduleRequest": {
    "taskId": "db0fc72e-4075-4106-ab06-b82c95ba08f7"
  }
}
```

## TEST 8: VERIFICĂ CĂ TASK-UL ESTE DIN NOU RUNNING
```json
{
  "action": "getrunning"
}
```

## TEST 9: CREEAZĂ UN TASK NOU PENTRU TESTE SUPLIMENTARE
```json
{
  "action": "create",
  "createRequest": {
    "name": "Task Nou pentru Teste",
    "description": "Al doilea task pentru a testa workflow-ul complet"
  }
}
```

## TEST 10: SCHEDULE TASK-UL NOU (folosește ID-ul din TEST 9)
```json
{
  "action": "schedule",
  "scheduleRequest": {
    "taskId": "db0fc72e-4075-4106-ab06-b82c95ba08f7"
  }
}
```

## TEST 11: VERIFICĂ CĂ AI 2 TASK-URI RUNNING
```json
{
  "action": "getrunning"
}
```

## TEST 12: DELETE TASK-UL ORIGINAL
```json
{
  "action": "delete",
  "taskId": "db0fc72e-4075-4106-ab06-b82c95ba08f7"
}
```

## TEST 13: VERIFICĂ CĂ A FOST ȘTERS
```json
{
  "action": "getall"
}
```

---

## RAPID FIRE TEST PENTRU RABBITMQ

### **Comandă 1:**
```json
{
  "action": "create",
  "createRequest": {
    "name": "RabbitMQ Test 1",
    "description": "Prima comandă pentru trafic"
  }
}
```

### **Comandă 2:** (folosește ID-ul din Comandă 1)
```json
{
  "action": "schedule",
  "scheduleRequest": {
    "taskId": "db0fc72e-4075-4106-ab06-b82c95ba08f7"
  }
}
```

### **Comandă 3:**
```json
{
  "action": "update",
  "taskId": "db0fc72e-4075-4106-ab06-b82c95ba08f7",
  "updateRequest": {
    "name": "RabbitMQ Test Actualizat",
    "description": "Actualizare pentru notificare fanout"
  }
}
```

### **Comandă 4:**
```json
{
  "action": "unschedule",
  "taskId": "db0fc72e-4075-4106-ab06-b82c95ba08f7"
}
```

### **Comandă 5:**
```json
{
  "action": "schedule",
  "scheduleRequest": {
    "taskId": "db0fc72e-4075-4106-ab06-b82c95ba08f7"
  }
}
```

### **Comandă 6:**
```json
{
  "action": "getrunning"
}
```

### **Comandă 7:**
```json
{
  "action": "delete",
  "taskId": "db0fc72e-4075-4106-ab06-b82c95ba08f7"
}
```

---

## Test complet de workflow:

### CREATE → UPDATE → SCHEDULE → GETALL → UNSCHEDULE → DELETE

```json
{
  "action": "create",
  "createRequest": {
    "name": "Task de test",
    "description": "Acesta este un task de test"
  }
}
```

```json
{
  "action": "update",
  "taskId": "db0fc72e-4075-4106-ab06-b82c95ba08f7",
  "updateRequest": {
    "name": "Task actualizat",
    "description": "Descriere nouă"
  }
}
```

```json
{
  "action": "schedule",
  "scheduleRequest": {
    "taskId": "db0fc72e-4075-4106-ab06-b82c95ba08f7"
  }
}
```

```json
{
  "action": "getall"
}
```

```json
{
  "action": "unschedule",
  "taskId": "db0fc72e-4075-4106-ab06-b82c95ba08f7"
}
```

```json
{
  "action": "delete",
  "taskId": "db0fc72e-4075-4106-ab06-b82c95ba08f7"
}
```

### Test RabbitMQ Activity:

```json
{
  "action": "create",
  "createRequest": {
    "name": "Test RabbitMQ Activity",
    "description": "Generez trafic pentru monitoring"
  }
}
```

```json
{
  "action": "schedule",
  "scheduleRequest": {
    "taskId": "db0fc72e-4075-4106-ab06-b82c95ba08f7"
  }
}
```

```json
{
  "action": "update",
  "taskId": "db0fc72e-4075-4106-ab06-b82c95ba08f7",
  "updateRequest": {
    "name": "Updated pentru RabbitMQ test",
    "description": "Descriere actualizată rapid"
  }
}
```

```json
{
  "action": "unschedule",
  "taskId": "db0fc72e-4075-4106-ab06-b82c95ba08f7"
}
```
