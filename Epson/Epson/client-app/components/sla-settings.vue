<template>
  <div class="sla-management-container" v-if="loggedInUser.roles.includes('Admin')">
    <v-card class="sla-card">
      <v-row>
        <v-col cols="12">
          <h2 class="blue-text">SLA CONFIGURATION</h2>
        </v-col>
        <v-col cols="12">
          <sla-settings />
        </v-col>
        <v-col cols="12" md="6">
          <sla-holiday />
        </v-col>
        <v-col cols="12" md="6">
          <sla-staffLeaves />
        </v-col>
      </v-row>
    </v-card>
  </div>
</template>

<script>
  import { mapGetters } from 'vuex';
  import slaStaffLeaves from '~/components/sla-staffleave.vue';
  import slaSettings from '~/components/sla-settings.vue';
  import slaHoliday from '~/components/sla-holiday.vue';

  export default {
    name: 'SLA',
    middleware: 'auth',
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser'])
    },
    components: {
      slaSettings,
      slaHoliday,
      slaStaffLeaves
    }
  };
</script>

<style scoped>
  .sla-management-container {
    display: flex;
    justify-content: center;
    align-items: center;
    padding: 20px;
    box-sizing: border-box;
  }

  .sla-card {
    width: 100%;
    max-width: 1200px;
    padding: 20px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
    border-radius: 8px;
  }

  .blue-text {
    color: #003399;
  }

  .v-row {
    margin: 0;
  }
</style>

<!-- sla-settings.vue -->
<template>
  <div class="sla-settings-container">
    <div class="card">
      <form>
        <div class="checkbox-group">
          <label>
            <input type="checkbox" v-model="workingHours">
            Include Working Hours
          </label>

          <div class="working-hours" v-if="workingHours">
            <label for="workingHoursStart">Start:</label>
            <input type="time" id="workingHoursStart" v-model="workingHoursStart">
            <label for="workingHoursEnd">End:</label>
            <input type="time" id="workingHoursEnd" v-model="workingHoursEnd">
          </div>
        </div>

        <div class="checkbox-group">
          <label>
            <input type="checkbox" id="holidays" v-model="holidays">
            Include Holidays
          </label>
        </div>

        <div class="checkbox-group">
          <label>
            <input type="checkbox" id="staffLeaves" v-model="staffLeaves">
            Include Staff Leaves
          </label>
        </div>

        <div class="form-actions">
          <button type="submit" class="save-btn" @click="saveSLASettings">SAVE</button>
          <button type="button" class="cancel-btn" @click="resetForm">CANCEL</button>
        </div>
      </form>
    </div>
  </div>
</template>

<script>
  import { mapGetters } from 'vuex';
  export default {
    name: 'SLA-Settings',
    middleware: "auth",
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser'])
    },
    data() {
      return {
        workingHours: false,
        workingHoursStart: null,
        workingHoursEnd: null,
        holidays: false,
        staffLeaves: false,
      };
    },
    methods: {
      resetForm() {
        this.workingHours = false;
        this.workingHoursStart = null;
        this.workingHoursEnd = null;
        this.holidays = false;
        this.staffLeaves = false;
      },
      async saveSLASettings() {
        try {
          await this.$axios.post(`${this.$config.restUrl}/api/sla/updateslasettings`, {
            data: {
              IncludeHoliday: this.holidays,
              IncludeStaffLeaves: this.staffLeaves,
              IncludeWorkingHours: this.workingHours,
              WorkingStartHour: parseInt(this.workingHoursStart.split(':')[0]),
              WorkingStartMinute: parseInt(this.workingHoursStart.split(':')[1]),
              WorkingEndHour: parseInt(this.workingHoursEnd.split(':')[0]),
              WorkingEndMinute: parseInt(this.workingHoursEnd.split(':')[1])
            }
          }).then(response => {
          })
        } catch (error) {
          console.error('There was a problem updating SLA settings');
        }
      },
      async getSLASettings() {
        try {
          this.loading = true
          await this.$axios.get(`${this.$config.restUrl}/api/sla/getslasettings`).then(result => {
            this.workingHours = result.data.data.includeWorkingHours
            this.workingHoursStart = `${result.data.data.workingStartHour.toString().padStart(2, '0')}:${result.data.data.workingStartMinute.toString().padStart(2, '0')}`;
            this.workingHoursEnd = `${result.data.data.workingEndHour.toString().padStart(2, '0')}:${result.data.data.workingEndMinute.toString().padStart(2, '0')}`;
            this.holidays = result.data.data.includeHoliday
            this.staffLeaves = result.data.data.includeStaffLeaves
          })

        } catch (error) {
          console.error('There was a problem fetching the SLA settings:', error);
        }
      }
    },
    mounted() {
      this.getSLASettings();
    },
    watch: {
      workingHours(value) {
        if (!value) {
          this.workingHoursStart = null;
          this.workingHoursEnd = null;
        }
      }
    }
  }
</script>

<style scoped>
  .sla-settings-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: flex-start;
    padding: 2rem;
  }

  .title {
    font-size: 1.5rem;
    color: #003399;
    margin-bottom: 1rem;
  }

  .card {
    background-color: #fff;
    border-radius: 10px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    padding: 2rem;
    max-width: 1200px;
    width: 100%;
  }

  form {
    display: flex;
    flex-direction: column;
    width: 100%;
  }

  .checkbox-group {
    display: flex;
    align-items: center;
    margin-bottom: 1rem;
  }

  label {
    display: flex;
    align-items: center;
    font-weight: bold;
    margin-right: 1rem;
  }

  input[type="checkbox"] {
    margin-right: 0.5rem;
    -webkit-appearance: none;
    -moz-appearance: none;
    appearance: none;
    border-radius: 3px;
    border: 2px solid #ccc;
    width: 1.2em;
    height: 1.2em;
  }

    input[type="checkbox"]:checked {
      background-color: #003399;
      border-color: #003399;
    }

  .working-hours {
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }

  input[type="time"] {
    padding: 0.25rem;
    border: 1px solid #ccc;
    border-radius: 0.25rem;
  }

  .form-actions {
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
    margin-top: 1rem;
  }

  .save-btn {
    background-color: #003399;
    color: #fff;
    border: none;
    padding: 0.5rem 1rem;
    border-radius: 5px;
    cursor: pointer;
  }

  .cancel-btn {
    background-color: #ccc;
    color: #fff;
    border: none;
    padding: 0.5rem 1rem;
    border-radius: 5px;
    cursor: pointer;
  }

  .save-btn:hover {
    background-color: #002366;
  }

  .cancel-btn:hover {
    background-color: #999;
  }
</style>
