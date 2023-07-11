<template>
  <v-dialog v-model="localDialogCompetitor" max-width="500px">
    <v-card>
      <v-card-title>
        <span class="headline">Add Competitor Information</span>
      </v-card-title>
      <v-card-text>
        <div class="form-group">
          <label>Model</label>
          <input type="text" v-model="localCompetitor.model" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
          <label>Brand</label>
          <input type="text" v-model="localCompetitor.brand" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
          <label>Price</label>
          <input v-model="localCompetitor.price" class="border-input" type="number" min="1" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
        </div>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="primary" @click="onAddCompetitor">Add</v-btn>
        <v-btn color="secondary" @click="onCancel">Cancel</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script>
  export default {
    props: {
      dialogCompetitor: Boolean,
      competitor: Object,
      isViewMode: Boolean
    },
    data() {
      return {
        localDialogCompetitor: this.dialogCompetitor,
        localCompetitor: { ...this.competitor }
      };
    },
    watch: {
      dialogCompetitor(newVal) {
        this.localDialogCompetitor = newVal;
      },
      localDialogCompetitor(newVal) {
        this.$emit('update:dialogCompetitor', newVal);
      },
      competitor: {
        handler(newVal) {
          this.localCompetitor = { ...newVal };
        },
        deep: true,
      }
    },
    methods: {
      onAddCompetitor() {
        if (this.localCompetitor.brand && this.localCompetitor.model && this.localCompetitor.price) {
          this.$emit('add-competitor', this.localCompetitor);
          this.localCompetitor = {
            model: null,
            brand: null,
            price: null,
          };
          this.localDialogCompetitor = false;
          
        } else {
          this.$swal('Error', 'Please fill out all competitor fields', 'error');
        }
      },
      onCancel() {
        this.localDialogCompetitor = false;
      }
    }
  };
</script>
