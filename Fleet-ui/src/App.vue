<template>
  <div class="app">
    <header class="header">
      <div class="header-inner">
        <div>
          <h1 class="header-title">FleetManager</h1>
        </div>
      </div>
    </header>

    <nav class="tabs">
      <button
        v-for="tab in tabs"
        :key="tab.id"
        class="tab-btn"
        :class="{ active: activeTab === tab.id }"
        @click="activeTab = tab.id"
      >
        {{ tab.label }}
      </button>
    </nav>

    <main class="content">
      <component :is="currentView" />
    </main>
  </div>
</template>

<script setup>
import { ref, computed, defineAsyncComponent } from 'vue'

const Vehicles        = defineAsyncComponent(() => import('./views/Vehicles.vue'))
const Fueling         = defineAsyncComponent(() => import('./views/Fueling.vue'))
const Maintenance     = defineAsyncComponent(() => import('./views/Maintenance.vue'))
const MaintenanceTypes = defineAsyncComponent(() => import('./views/MaintenanceTypes.vue'))

const tabs = [
  { id: 'vehicles',  label: 'Pojazdy'},
  { id: 'fueling',   label: 'Tankowania'},
  { id: 'maintenance', label: 'Przeglądy'},
  { id: 'types',     label: 'Typy przeglądów'},
]

const activeTab = ref('vehicles')

const views = { vehicles: Vehicles, fueling: Fueling, maintenance: Maintenance, types: MaintenanceTypes }
const currentView = computed(() => views[activeTab.value])
</script>

<style>
/* ── Reset ── */
*, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
body { font-family: system-ui, -apple-system, sans-serif; background: #f0f3f8; color: #1a1a2e; line-height: 1.5; }

/* ── Layout ── */
.app { min-height: 100vh; display: flex; flex-direction: column; }

.header { background: linear-gradient(135deg, #0f2649 0%, #1e5fa8 100%); color: white; padding: 18px 32px; }
.header-inner { display: flex; justify-content: space-between; align-items: center; }
.header-title { font-size: 1.7rem; font-weight: 800; letter-spacing: -0.5px; }
.header-sub { font-size: 0.82rem; opacity: 0.75; margin-top: 2px; }
.header-meta { font-size: 0.78rem; opacity: 0.6; }

.tabs { background: white; border-bottom: 2px solid #e4e8ef; padding: 0 28px; display: flex; gap: 2px; box-shadow: 0 2px 6px rgba(0,0,0,0.04); }
.tab-btn { padding: 13px 20px; border: none; background: none; font-size: 0.9rem; font-weight: 500; color: #6b7280; cursor: pointer; border-bottom: 3px solid transparent; margin-bottom: -2px; transition: color 0.15s, border-color 0.15s; display: flex; align-items: center; gap: 6px; }
.tab-btn:hover { color: #1e5fa8; }
.tab-btn.active { color: #1e5fa8; border-bottom-color: #1e5fa8; font-weight: 600; }
.tab-icon { font-size: 1rem; }

.content { flex: 1; padding: 24px 32px; max-width: 1400px; width: 100%; margin: 0 auto; }

/* ── Cards ── */
.card { background: white; border-radius: 10px; box-shadow: 0 1px 4px rgba(0,0,0,0.08); padding: 22px 24px; margin-bottom: 20px; }
.card-title { font-size: 1.05rem; font-weight: 700; color: #0f2649; margin-bottom: 18px; }

/* ── Buttons ── */
.btn { padding: 8px 16px; border: none; border-radius: 7px; font-size: 0.85rem; font-weight: 600; cursor: pointer; transition: filter 0.15s, transform 0.1s; }
.btn:hover:not(:disabled) { filter: brightness(1.1); }
.btn:active:not(:disabled) { transform: scale(0.97); }
.btn:disabled { opacity: 0.5; cursor: not-allowed; }
.btn-primary   { background: #1e5fa8; color: white; }
.btn-success   { background: #16a34a; color: white; }
.btn-warning   { background: #d97706; color: white; }
.btn-danger    { background: #dc2626; color: white; }
.btn-secondary { background: #6b7280; color: white; }
.btn-sm { padding: 4px 10px; font-size: 0.78rem; }

/* ── Table ── */
table { width: 100%; border-collapse: collapse; font-size: 0.875rem; }
th { text-align: left; padding: 10px 14px; background: #f8fafc; color: #4b5563; font-weight: 600; font-size: 0.8rem; text-transform: uppercase; letter-spacing: 0.04em; border-bottom: 2px solid #e4e8ef; }
td { padding: 11px 14px; border-bottom: 1px solid #f1f4f9; vertical-align: middle; }
tr:last-child td { border-bottom: none; }
tr:hover td { background: #f5f8ff; }

/* ── Badges ── */
.badge { display: inline-block; padding: 3px 10px; border-radius: 20px; font-size: 0.75rem; font-weight: 700; }
.badge-green  { background: #dcfce7; color: #166534; }
.badge-blue   { background: #dbeafe; color: #1e40af; }
.badge-orange { background: #fff7ed; color: #9a3412; }
.badge-red    { background: #fee2e2; color: #991b1b; }
.badge-gray   { background: #f3f4f6; color: #374151; }

/* ── Forms ── */
.form-row { display: flex; gap: 14px; flex-wrap: wrap; margin-bottom: 14px; }
.form-group { display: flex; flex-direction: column; gap: 5px; flex: 1; min-width: 150px; }
.form-group label { font-size: 0.78rem; font-weight: 700; color: #4b5563; text-transform: uppercase; letter-spacing: 0.04em; }
.form-group input, .form-group select, .form-group textarea {
  padding: 9px 11px; border: 1.5px solid #d1d5db; border-radius: 7px;
  font-size: 0.875rem; outline: none; transition: border-color 0.15s, box-shadow 0.15s;
  background: white;
}
.form-group input:focus, .form-group select:focus, .form-group textarea:focus {
  border-color: #1e5fa8; box-shadow: 0 0 0 3px rgba(30,95,168,0.12);
}

/* ── Modal ── */
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.45); display: flex; align-items: center; justify-content: center; z-index: 200; backdrop-filter: blur(2px); }
.modal { background: white; border-radius: 12px; padding: 28px; width: 580px; max-width: 95vw; max-height: 90vh; overflow-y: auto; box-shadow: 0 20px 60px rgba(0,0,0,0.2); }
.modal-title { font-size: 1.1rem; font-weight: 800; color: #0f2649; margin-bottom: 22px; padding-bottom: 14px; border-bottom: 2px solid #f1f4f9; }
.modal-actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 20px; padding-top: 16px; border-top: 1px solid #f1f4f9; }

/* ── Alerts ── */
.error-msg   { color: #991b1b; font-size: 0.83rem; background: #fee2e2; padding: 10px 14px; border-radius: 7px; margin-top: 10px; border-left: 3px solid #dc2626; }
.success-msg { color: #166534; font-size: 0.83rem; background: #dcfce7; padding: 10px 14px; border-radius: 7px; margin-top: 10px; border-left: 3px solid #16a34a; }

/* ── Utility ── */
.section-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 18px; }
.actions { display: flex; gap: 5px; flex-wrap: wrap; }
.loading { text-align: center; color: #9ca3af; padding: 40px; font-size: 0.9rem; }
.empty { text-align: center; color: #9ca3af; padding: 36px; font-style: italic; }

/* ── Stat grid ── */
.stat-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(160px, 1fr)); gap: 14px; }
.stat-card { background: #f5f8ff; border-radius: 10px; padding: 16px; text-align: center; border: 1px solid #e4eeff; }
.stat-card .stat-value { font-size: 1.5rem; font-weight: 800; color: #1e5fa8; line-height: 1.1; }
.stat-card .stat-label { font-size: 0.72rem; color: #6b7280; margin-top: 4px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.04em; }

/* ── Maintenance status list ── */
.status-list { list-style: none; display: flex; flex-direction: column; gap: 10px; }
.status-item { padding: 14px 16px; border-radius: 8px; border-left: 4px solid #d1d5db; background: #f9fafb; font-size: 0.875rem; }
.status-item.ok      { border-left-color: #16a34a; background: #f0fdf4; }
.status-item.warn    { border-left-color: #d97706; background: #fffbeb; }
.status-item.overdue { border-left-color: #dc2626; background: #fef2f2; }
.status-item-name { font-weight: 700; color: #1a1a2e; }
.status-item-msg  { color: #4b5563; margin-top: 4px; font-size: 0.82rem; }
.status-item-meta { color: #9ca3af; margin-top: 3px; font-size: 0.8rem; }
</style>
