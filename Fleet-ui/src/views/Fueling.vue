<template>
  <div>
    <!-- Selector -->
    <div class="card">
      <div class="section-header">
        <h2 class="card-title" style="margin:0">Tankowania</h2>
      </div>
      <div class="form-row" style="margin-bottom:0">
        <div class="form-group" style="max-width:340px">
          <label>Wybierz pojazd</label>
          <select v-model="selectedId" @change="onVehicleChange">
            <option value="">— wybierz pojazd —</option>
            <option v-for="v in vehicles" :key="v.id" :value="v.id">
              {{ v.brand }} {{ v.model }} · {{ v.licensePlate }}
            </option>
          </select>
        </div>
      </div>
    </div>

    <template v-if="selectedId">
      <!-- Fuel statistics -->
      <div class="card" v-if="fuelStats">
        <h3 class="card-title">Statystyki paliwa</h3>
        <div class="stat-grid">
          <div class="stat-card">
            <div class="stat-value">{{ fuelStats.totalDistanceKm?.toLocaleString('pl-PL') }}</div>
            <div class="stat-label">Km łącznie</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ fuelStats.totalFuelLiters?.toFixed(1) }} L</div>
            <div class="stat-label">Paliwo łącznie</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ fuelStats.totalCost?.toFixed(2) }} zł</div>
            <div class="stat-label">Koszt łączny</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ fuelStats.averageConsumption?.toFixed(2) }}</div>
            <div class="stat-label">L / 100 km</div>
          </div>
        </div>
      </div>

      <!-- Add fueling event -->
      <div class="card">
        <h3 class="card-title">Dodaj tankowanie</h3>
        <div class="form-row">
          <div class="form-group">
            <label>Data i godzina *</label>
            <input v-model="addForm.date" type="datetime-local" />
          </div>
          <div class="form-group">
            <label>Stan licznika (km) *</label>
            <input v-model.number="addForm.odometerReading" type="number" min="0" placeholder="np. 50000" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Litry zatankowane * (0.01 – 1000)</label>
            <input v-model.number="addForm.litersAdded" type="number" step="0.01" min="0.01" max="1000" placeholder="np. 45.5" />
          </div>
          <div class="form-group">
            <label>Koszt (zł) * (0.01 – 100 000)</label>
            <input v-model.number="addForm.cost" type="number" step="0.01" min="0.01" max="100000" placeholder="np. 280.00" />
          </div>
          <div class="form-group" style="max-width:120px;justify-content:flex-end;padding-top:22px">
            <div style="background:#f5f8ff;border-radius:7px;padding:9px 11px;text-align:center;border:1.5px solid #e4eeff">
              <div style="font-size:1rem;font-weight:800;color:#1e5fa8">
                {{ addForm.litersAdded && addForm.cost ? (addForm.cost / addForm.litersAdded).toFixed(2) : '—' }}
              </div>
              <div style="font-size:0.68rem;color:#9ca3af;font-weight:600;text-transform:uppercase">zł / L</div>
            </div>
          </div>
        </div>
        <div v-if="addError" class="error-msg">{{ addError }}</div>
        <button class="btn btn-primary" @click="submitAdd" :disabled="saving">
          {{ saving ? 'Dodawanie...' : '+ Dodaj tankowanie' }}
        </button>
      </div>

      <!-- History -->
      <div class="card">
        <h3 class="card-title">Historia tankowań</h3>
        <div v-if="loadingHistory" class="loading">Ładowanie historii...</div>
        <table v-else>
          <thead>
            <tr>
              <th>ID</th>
              <th>Data</th>
              <th>Licznik (km)</th>
              <th>Litry</th>
              <th>Koszt (zł)</th>
              <th>Cena / L (zł)</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="!history.length">
              <td colspan="7" class="empty">Brak zdarzeń tankowania</td>
            </tr>
            <tr v-for="e in history" :key="e.id">
              <td style="color:#9ca3af;font-size:0.8rem">#{{ e.id }}</td>
              <td>{{ fmtDateTime(e.date) }}</td>
              <td>{{ e.odometerReading.toLocaleString('pl-PL') }}</td>
              <td>{{ e.litersAdded.toFixed(2) }} L</td>
              <td>{{ e.cost.toFixed(2) }} zł</td>
              <td>{{ (e.cost / e.litersAdded).toFixed(2) }} zł</td>
              <td>
                <button class="btn btn-danger btn-sm" @click="doDelete(e)">Usuń</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <div v-else class="card" style="text-align:center;color:#9ca3af;padding:48px">
      Wybierz pojazd, aby zobaczyć historię tankowań
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { vehiclesApi, fuelingApi } from '../api.js'

const vehicles       = ref([])
const selectedId     = ref('')
const history        = ref([])
const fuelStats      = ref(null)
const loadingHistory = ref(false)
const saving         = ref(false)
const addError       = ref('')

function freshForm() {
  return {
    date: new Date().toISOString().slice(0, 16),
    odometerReading: 0,
    litersAdded: null,
    cost: null,
  }
}
const addForm = ref(freshForm())

async function onVehicleChange() {
  if (!selectedId.value) return
  loadingHistory.value = true
  fuelStats.value = null
  try {
    ;[history.value, fuelStats.value] = await Promise.all([
      fuelingApi.getByVehicle(selectedId.value),
      vehiclesApi.getFuelStats(selectedId.value),
    ])
  } catch (e) { console.error(e) }
  finally { loadingHistory.value = false }
}

async function submitAdd() {
  saving.value = true; addError.value = ''
  try {
    await fuelingApi.create({
      vehicleId: Number(selectedId.value),
      odometerReading: addForm.value.odometerReading,
      litersAdded: addForm.value.litersAdded,
      cost: addForm.value.cost,
      date: new Date(addForm.value.date).toISOString(),
    })
    addForm.value = freshForm()
    await onVehicleChange()
  } catch (e) { addError.value = e.message }
  finally { saving.value = false }
}

async function doDelete(e) {
  if (!confirm('Usunąć to zdarzenie tankowania?')) return
  try { await fuelingApi.delete(e.id); await onVehicleChange() }
  catch (err) { alert(err.message) }
}

function fmtDateTime(d) { return new Date(d).toLocaleString('pl-PL') }

onMounted(async () => {
  try { vehicles.value = await vehiclesApi.getAll() } catch {}
})
</script>
