<template>
  <div class="chart-wrapper">
    <canvas ref="canvas"></canvas>
  </div>
</template>

<script>
  import { Doughnut, mixins } from 'vue-chartjs'

  export default {
    extends: Doughnut,
    mixins: [mixins.reactiveProp],
    props: ['chartData', 'options'],
    mounted() {
      this.renderChart(this.chartData, this.options);
      window.addEventListener('resize', this.handleResize);
    },
    beforeDestroy() {
      window.removeEventListener('resize', this.handleResize);
    },
    methods: {
      handleResize() {
        if (this.$data._chart) {
          this.$data._chart.resize();
        }
      }
    },
    watch: {
      chartData() {
        this.$data._chart.update();
        this.handleResize();
      }
    }
  }
</script>

<style scoped>
  .chart-wrapper {
    position: relative;
    width: 100%;
    height: 100%;
  }

  canvas {
    width: 100% !important;
    height: 100% !important;
  }
</style>
